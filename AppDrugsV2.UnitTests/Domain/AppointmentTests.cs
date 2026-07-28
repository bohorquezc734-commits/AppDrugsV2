using AppDrugsV2.Domain.Entities;
using AppDrugsV2.Domain.Enums;
using FluentAssertions;
using NUnit.Framework;
using System;

namespace AppDrugsV2.UnitTests.Domain
{
    [TestFixture]
    public class AppointmentTests
    {
        [Test]
        public void Constructor_WithValidArguments_ShouldCreateAppointmentInRecibidoStatus()
        {
            int userId = 1;
            int gestorId = 2;

            var appointment = new Appointment(userId, gestorId);

            appointment.UserId.Should().Be(userId);
            appointment.GestorFarmaceuticoId.Should().Be(gestorId);
            appointment.Status.Should().Be(AppointmentStatus.Recibido);
            appointment.IsActive.Should().BeTrue();
            appointment.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void Constructor_WithInvalidUserId_ShouldThrowArgumentException(int invalidUserId)
        {
            Action act = () => new Appointment(invalidUserId, 2);

            act.Should().Throw<ArgumentException>()
               .WithMessage("*ID del usuario*");
        }

        [Test]
        public void Confirmar_WhenStatusIsRecibido_ShouldChangeStatusToEnProceso()
        {
        
            var appointment = new Appointment(1, 2);

            appointment.Confirmar();

            appointment.Status.Should().Be(AppointmentStatus.EnProceso);
        }

        [Test]
        public void Confirmar_WhenStatusIsNotRecibido_ShouldThrowInvalidOperationException()
        {
            var appointment = new Appointment(1, 2);
            appointment.Confirmar(); 

            Action act = () => appointment.Confirmar();

            act.Should().Throw<InvalidOperationException>()
               .WithMessage("Solo se pueden confirmar turnos en estado 'Recibido'.");
        }

        [Test]
        public void Entregar_WhenStatusIsEnProceso_ShouldChangeStatusToEntregado()
        {
            var appointment = new Appointment(1, 2);
            appointment.Confirmar();

            appointment.Entregar();

            appointment.Status.Should().Be(AppointmentStatus.Entregado);
            appointment.FechaEntrega.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Test]
        public void Cancelar_WhenStatusIsNotEntregado_ShouldChangeStatusToCancelado()
        {
            var appointment = new Appointment(1, 2);
            var observaciones = "Falta receta";

            appointment.Cancelar(observaciones);

           
            appointment.Status.Should().Be(AppointmentStatus.Cancelado);
            appointment.Observaciones.Should().Be(observaciones);
        }

        [Test]
        public void Cancelar_WhenStatusIsEntregado_ShouldThrowInvalidOperationException()
        {
            var appointment = new Appointment(1, 2);
            appointment.Confirmar();
            appointment.Entregar();

            Action act = () => appointment.Cancelar("Test");

            act.Should().Throw<InvalidOperationException>()
               .WithMessage("No se puede cancelar un turno ya entregado.");
        }
    }
}
