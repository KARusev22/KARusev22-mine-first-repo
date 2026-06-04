using CanteenReservationSystem.Models.ViewModels;

namespace CanteenReservationSystem.Services.Interfaces;

public interface IKitchenService
{
    KitchenViewModel GetKitchenData(DateTime date);
}