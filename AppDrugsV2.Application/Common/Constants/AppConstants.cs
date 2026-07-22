namespace AppDrugsV2.Application.Common.Constants
{
    /// <summary>
    /// Constantes centralizadas de la aplicación.
    /// Elimina textos quemados y números mágicos.
    /// </summary>
    public static class AppConstants
    {
        // ─── ROLES ────────────────────────────────────────────────────────────────
        public static class Roles
        {
            public const string Admin      = "Admin";
            public const string Pharmacist = "Pharmacist";
            public const string User       = "User";

            /// <summary>Rol por defecto al registrar un usuario.</summary>
            public const string Default = User;

            public const string AdminOrPharmacist = $"{Admin},{Pharmacist}";
            public const string UserOrAdmin       = $"{User},{Admin}";
        }

        // ─── JWT ──────────────────────────────────────────────────────────────────
        public static class Jwt
        {
            public const string SectionName         = "JwtSettings";
            public const string SecretKey           = "Secret";
            public const string IssuerKey           = "Issuer";
            public const string AudienceKey         = "Audience";
            public const string DefaultIssuer       = "AppDrugsV2";
            public const string DefaultAudience     = "AppDrugsV2Client";
            public const string BearerScheme        = "Bearer";
            public const string AuthorizationHeader = "Authorization";
            public const string BearerDescription   =
                "JWT Authorization header usando el esquema Bearer. Ejemplo: \"Authorization: Bearer {token}\"";
            public const string ErrorJwtNotConfigured = "JWT Secret no configurado";

            /// <summary>Horas de validez del token JWT.</summary>
            public const int TokenExpirationHours = 1;
        }

        // ─── CORS ─────────────────────────────────────────────────────────────────
        public static class Cors
        {
            public const string PolicyName      = "AllowReactApp";
            public const string FrontendOrigin  = "http://localhost:3000";
        }

        // ─── SWAGGER ──────────────────────────────────────────────────────────────
        public static class Swagger
        {
            public const string DocVersion = "v1";
            public const string DocTitle   = "AppDrugsV2 API";
        }

        // ─── ROTATIVA / PDF ───────────────────────────────────────────────────────
        public static class Rotativa
        {
            public const string FolderName          = "Rotativa";
            public const string DisableSmartShrinking = "--disable-smart-shrinking";
            public const int    MarginTop            = 10;
            public const int    MarginBottom         = 10;
            public const int    MarginLeft           = 15;
            public const int    MarginRight          = 15;

            public static class Views
            {
                public const string AppointmentReport = "AppointmentReport";
                public const string InventoryReport   = "InventoryReport";
            }

            public static class FileNames
            {
                public const string AppointmentsPrefix = "Turnos";
                public const string InventoryPrefix    = "Inventario";
                public const string DateFormat         = "yyyyMMdd_HHmm";
            }
        }

        // ─── CONTENT TYPES ────────────────────────────────────────────────────────
        public static class ContentTypes
        {
            public const string Excel = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            public const string Pdf   = "application/pdf";
        }

        // ─── VALIDACIÓN: LÍMITES ──────────────────────────────────────────────────
        public static class Validation
        {
            public const int EmailMaxLength    = 100;
            public const int FullNameMaxLength = 100;
            public const int PasswordMinLength = 8;

            /// <summary>Umbral de stock mínimo; debajo de este valor se dispara DrugLowStockEvent.</summary>
            public const int LowStockThreshold = 10;

            public const string RegexUpperCase = "[A-Z]";
            public const string RegexLowerCase = "[a-z]";
            public const string RegexDigit     = "[0-9]";
        }

        // ─── PAGINACIÓN ────────────────────────────────────────────────────────────────
        public static class Pagination
        {
            /// <summary>Máximo de notificaciones devueltas por consulta.</summary>
            public const int NotificationsPageSize = 20;
        }

        // ─── MENSAJES DE RESPUESTA ────────────────────────────────────────────────
        public static class Messages
        {
            // Éxito
            public const string UserRegistered       = "Usuario registrado exitosamente";
            public const string DrugUpdated          = "Medicamento actualizado exitosamente";
            public const string DrugDeleted          = "Medicamento eliminado exitosamente";
            public const string StockUpdated         = "Stock actualizado exitosamente";
            public const string StockAdded           = "Stock agregado exitosamente";
            public const string StockRemoved         = "Stock removido exitosamente";
            public const string InventoryUpdated     = "Inventario actualizado exitosamente";
            public const string InventoryDeleted     = "Inventario eliminado exitosamente";
            public const string AppointmentUpdated   = "Estado del turno actualizado exitosamente.";
            public const string PasswordChanged      = "Contraseña actualizada exitosamente.";
            public const string ProfileUpdated       = "Perfil actualizado exitosamente.";

            // Error — autenticación / sesión
            public const string UserNotAuthenticated    = "Usuario no autenticado.";
            public const string UserNotAuthenticatedEn = "User is not authenticated";
            public const string InvalidCredentials      = "Credenciales inválidas";
            public const string UserDeactivated         = "Usuario desactivado";
            public const string InvalidResetToken       = "El código de recuperación es inválido o ha expirado.";
            public const string InvalidAppointmentStatus = "Estado inválido para esta operación.";
            public const string DefaultCancellationReason = "Cancelado por el gestor.";

            // Error — entidades no encontradas
            public const string IdMismatch              = "El ID en la URL no coincide con el ID en el cuerpo.";
            public const string EmailAlreadyRegistered  = "El email ya está registrado";
            public const string NotFoundKeyword         = "no encontrado";
            public const string NotExistsKeyword        = "no existe";
            public const string UserNotFound            = "Usuario no encontrado.";
            public const string NotificationNotFound    = "Notification not found";
            public const string CurrentPasswordIncorrect = "La contraseña actual es incorrecta.";
            public const string DuplicateInventory      = "Ya existe un inventario para este medicamento en esta sede";

            // Validación
            public const string EmailRequired        = "Email es requerido";
            public const string EmailInvalidFormat   = "Formato de email inválido";
            public const string EmailMaxLengthMsg    = "Email no debe exceder 100 caracteres";
            public const string PasswordRequired     = "Password es requerido";
            public const string PasswordMinLengthMsg = "Password debe tener al menos 8 caracteres";
            public const string PasswordUpperCase    = "Password debe tener al menos una mayúscula";
            public const string PasswordLowerCase    = "Password debe tener al menos una minúscula";
            public const string PasswordDigit        = "Password debe tener al menos un número";
            public const string FullNameRequired     = "Nombre completo es requerido";
            public const string FullNameMaxLengthMsg = "Nombre no debe exceder 100 caracteres";
            public const string RoleInvalid          = "Rol inválido. Debe ser User, Pharmacist o Admin";
        }

        // ─── NOTIFICACIONES (mensajes al usuario) ─────────────────────────────────
        public static class NotificationMessages
        {
            public const string AppointmentRecibido  = "📋 Tu turno #{0} ha sido actualizado al estado: Recibido.";
            public const string AppointmentEnProceso = "⚙️ Tu turno #{0} está En Proceso. Pronto estará listo.";
            public const string AppointmentEntregado = "✅ Tu turno #{0} ha sido Entregado exitosamente. ¡Gracias!";
            public const string AppointmentCancelado = "❌ Tu turno #{0} fue Cancelado. Motivo: {1}";
            public const string AppointmentCreado    = "✅ Tu turno #{0} en {1} fue registrado exitosamente. Estado: Recibido.";
        }

        // ─── CLAIMS ───────────────────────────────────────────────────────────────
        public static class Claims
        {
            public const string NameIdentifier = "nameidentifier";
        }
    }
}
