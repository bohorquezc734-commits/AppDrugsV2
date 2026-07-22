import { create } from 'zustand';

export type DrugiAnimationType = 'saludo' | 'pensando' | 'hablando' | 'feliz' | 'sorpresa' | 'triste';

export interface ChatMessage {
  id: string;
  sender: 'user' | 'drugi';
  text: string;
}

interface DrugiState {
  isOpen: boolean;
  messages: ChatMessage[];
  animationType: DrugiAnimationType;
  open: () => void;
  close: () => void;
  toggle: () => void;
  addMessage: (msg: ChatMessage) => void;
  showMessage: (msg: string, anim?: DrugiAnimationType) => void;
}

export const useDrugiStore = create<DrugiState>((set) => ({
  isOpen: false,
  messages: [
    { id: '1', sender: 'drugi', text: '¡Hola! 👋\n¿En qué puedo ayudarte hoy?' }
  ],
  animationType: 'saludo',
  open: () => set({ isOpen: true }),
  close: () => set({ isOpen: false }),
  toggle: () => set((state) => ({ isOpen: !state.isOpen })),
  addMessage: (msg) => set((state) => ({ messages: [...state.messages, msg] })),
  showMessage: (msg, anim = 'hablando') => set((state) => ({ 
    isOpen: true, 
    animationType: anim,
    messages: [...state.messages, { id: Date.now().toString(), sender: 'drugi', text: msg }]
  })),
}));
