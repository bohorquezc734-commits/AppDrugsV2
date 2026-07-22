import React, { useState, useRef, useEffect, useCallback } from 'react';
import { Transition } from '@headlessui/react';
import { useDrugiStore } from '../../store/useDrugiStore';
import { getDrugiResponse } from '../../services/drugiService';
import { authService } from '../../services/auth';

// Las 2 únicas imágenes: idénticas, solo cambian los ojos
import DrugiOpen from '../../assets/drugi_open.png';
import DrugiClosed from '../../assets/drugi_closed.png';

// ── Parpadeo ──────────────────────────────────────────────────────────────────
// Ciclo: esperar ~3.5s con ojos ABIERTOS, luego 150ms CERRADOS, repite.
function useBlink() {
  const [isBlinking, setIsBlinking] = useState(false);

  useEffect(() => {
    let timeout: ReturnType<typeof setTimeout>;

    const blink = () => {
      // Cerrar ojos durante 100 ms (más rápido)
      setIsBlinking(true);
      timeout = setTimeout(() => {
        setIsBlinking(false);
        // Esperar entre 400 y 800 ms para el próximo parpadeo
        const nextBlink = 400 + Math.random() * 400;
        timeout = setTimeout(blink, nextBlink);
      }, 100);
    };

    // Primer parpadeo después de 500ms de cargar
    timeout = setTimeout(blink, 500);
    return () => clearTimeout(timeout);
  }, []);

  return isBlinking;
}

// ── Avatar component ──────────────────────────────────────────────────────────
interface AvatarProps { size?: number; className?: string; }

const DrugiAvatar: React.FC<AvatarProps> = ({ size = 64, className = '' }) => {
  const isBlinking = useBlink();

  return (
    <div
      className={`rounded-full overflow-hidden flex-shrink-0 ${className}`}
      style={{ width: size, height: size }}
    >
      <img
        src={isBlinking ? DrugiClosed : DrugiOpen}
        alt="Drugi"
        className="w-full h-full object-cover select-none"
        draggable={false}
      />
    </div>
  );
};

// ── CSS global (una sola vez) ─────────────────────────────────────────────────
const STYLE = `
  @keyframes drugiPing {
    0%   { transform: scale(1);   opacity: .35; }
    70%  { transform: scale(1.7); opacity: 0;   }
    100% { transform: scale(1.7); opacity: 0;   }
  }
  .drugi-ping { animation: drugiPing 2.2s ease-out infinite; }
`;

// ─────────────────────────────────────────────────────────────────────────────

export const DrugiAssistant: React.FC = () => {
  const { isOpen, messages, toggle, close, addMessage } = useDrugiStore();
  const [inputValue, setInputValue] = useState('');
  const [isTyping, setIsTyping] = useState(false);
  const messagesEndRef = useRef<HTMLDivElement>(null);

  const user = authService.getUser();
  const userName = user?.name?.split(' ')[0] || 'Usuario';

  // Inyectar CSS una sola vez
  useEffect(() => {
    if (!document.getElementById('drugi-styles')) {
      const tag = document.createElement('style');
      tag.id = 'drugi-styles';
      tag.textContent = STYLE;
      document.head.appendChild(tag);
    }
  }, []);

  // Auto-scroll
  useEffect(() => {
    if (isOpen) messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages, isOpen, isTyping]);

  const handleSend = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!inputValue.trim() || isTyping) return;

    const userText = inputValue.trim();
    addMessage({ id: Date.now().toString(), sender: 'user', text: userText });
    setInputValue('');
    setIsTyping(true);

    const response = await getDrugiResponse(userText);
    setIsTyping(false);
    addMessage({ id: (Date.now() + 1).toString(), sender: 'drugi', text: response });
  };

  const unreadCount = messages.filter(m => m.sender === 'drugi').length;

  return (
    <div className="fixed bottom-6 right-6 z-50 flex flex-col items-end">

      {/* ── Ventana de Chat ── */}
      <Transition
        show={isOpen}
        enter="transition-all ease-out duration-300"
        enterFrom="opacity-0 translate-y-4 scale-95"
        enterTo="opacity-100 translate-y-0 scale-100"
        leave="transition-all ease-in duration-200"
        leaveFrom="opacity-100 translate-y-0 scale-100"
        leaveTo="opacity-0 translate-y-4 scale-95"
      >
        <div className="bg-white shadow-2xl rounded-2xl mb-4 w-[355px] h-[520px] border border-gray-100 flex flex-col overflow-hidden">

          {/* Header */}
          <div className="bg-[#2563eb] text-white px-4 py-3 flex items-center justify-between rounded-t-2xl">
            <div className="flex items-center gap-3">
              <DrugiAvatar size={44} className="border-2 border-white/50 shadow" />
              <div>
                <p className="font-bold text-sm">¡Hola {userName}! 👋</p>
                <p className="text-xs text-blue-100 flex items-center gap-1">
                  <span className="w-1.5 h-1.5 bg-green-400 rounded-full inline-block"></span>
                  En línea · Asistente farmacéutico
                </p>
              </div>
            </div>
            <button
              onClick={close}
              className="text-blue-100 hover:text-white bg-blue-700/50 hover:bg-blue-700 p-1.5 rounded-full transition-colors"
            >
              <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none"
                stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                <line x1="18" y1="6" x2="6" y2="18" /><line x1="6" y1="6" x2="18" y2="18" />
              </svg>
            </button>
          </div>

          {/* Mensajes */}
          <div className="flex-1 bg-[#f0f4ff] overflow-y-auto p-4 space-y-3">
            {messages.map((msg) => (
              <div key={msg.id} className={`flex items-end gap-2 ${msg.sender === 'user' ? 'justify-end' : 'justify-start'}`}>
                {msg.sender === 'drugi' && (
                  <DrugiAvatar size={32} className="border border-blue-100 bg-white shadow-sm" />
                )}
                <div className={`max-w-[75%] px-3 py-2 text-sm shadow-sm ${msg.sender === 'user'
                  ? 'bg-[#2563eb] text-white rounded-2xl rounded-br-none'
                  : 'bg-white text-gray-700 rounded-2xl rounded-bl-none border border-gray-100'
                  }`}>
                  <p className="whitespace-pre-line leading-relaxed">{msg.text}</p>
                </div>
              </div>
            ))}

            {/* Typing dots */}
            {isTyping && (
              <div className="flex items-end gap-2">
                <DrugiAvatar size={32} className="border border-blue-100 bg-white shadow-sm" />
                <div className="bg-white border border-gray-100 rounded-2xl rounded-bl-none px-4 py-3 shadow-sm flex gap-1">
                  {[0, 150, 300].map(d => (
                    <span key={d} className="w-2 h-2 bg-blue-400 rounded-full animate-bounce"
                      style={{ animationDelay: `${d}ms` }} />
                  ))}
                </div>
              </div>
            )}
            <div ref={messagesEndRef} />
          </div>

          {/* Input */}
          <div className="p-3 bg-white border-t border-gray-100">
            <form onSubmit={handleSend} className="relative flex items-center">
              <input
                type="text"
                value={inputValue}
                onChange={e => setInputValue(e.target.value)}
                placeholder="Escribe tu pregunta..."
                disabled={isTyping}
                className="flex-1 bg-gray-50 border border-gray-200 rounded-full py-2.5 pl-4 pr-12 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent disabled:opacity-50 transition-all"
              />
              <button
                type="submit"
                disabled={!inputValue.trim() || isTyping}
                className="absolute right-1.5 w-8 h-8 bg-[#2563eb] text-white rounded-full flex items-center justify-center hover:bg-blue-700 disabled:opacity-40 transition-colors"
              >
                <svg className="w-4 h-4 ml-0.5" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24"
                  fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                  <line x1="22" y1="2" x2="11" y2="13" />
                  <polygon points="22 2 15 22 11 13 2 9 22 2" />
                </svg>
              </button>
            </form>
          </div>

        </div>
      </Transition>

      {/* ── Botón flotante ── */}
      <button
        onClick={toggle}
        className="relative flex items-center justify-center hover:scale-110 active:scale-95 transition-transform"
        style={{ width: 72, height: 72 }}
      >
        {/* Anillo de vida */}
        <span className="drugi-ping absolute inset-0 rounded-full bg-blue-400 pointer-events-none" />

        <DrugiAvatar size={68} className="shadow-xl border-[3px] border-white relative z-10" />

        {!isOpen && unreadCount > 1 && (
          <span className="absolute top-0.5 right-0.5 min-w-[18px] h-[18px] bg-red-500 text-white text-[9px] font-bold rounded-full border-2 border-white flex items-center justify-center z-20 px-0.5">
            {unreadCount}
          </span>
        )}
      </button>
    </div>
  );
};
