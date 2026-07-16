using HomemadeFood.Api.Entities;
using HomemadeFood.Api.Interfaces;

namespace HomemadeFood.Api.Services
{
    public sealed class ProducerCapacityService
        : IProducerCapacityService
    {
        private readonly IAppClock _appClock;

        public ProducerCapacityService(
            IAppClock appClock)
        {
            _appClock = appClock;
        }

        public void EnsureCurrentDay(
            ProducerProfile producerProfile)
        {
            var today = _appClock.TurkeyToday;

            if (producerProfile.CapacityDate == today)
            {
                return;
            }

            producerProfile.RemainingCapacity =
                producerProfile.DailyCapacity;

            producerProfile.CapacityDate = today;
        }

        public bool TryReserve(
            ProducerProfile producerProfile,
            int quantity)
        {
            if (quantity <= 0)
            {
                return false;
            }

            EnsureCurrentDay(producerProfile);

            if (producerProfile.RemainingCapacity <
                quantity)
            {
                return false;
            }

            producerProfile.RemainingCapacity -= quantity;

            return true;
        }

        public void RestoreForOrder(
            ProducerProfile producerProfile,
            DateTime orderCreatedAt,
            int quantity)
        {
            if (quantity <= 0)
            {
                return;
            }

            /*
             * Önce üreticinin kapasitesini bugüne geçirir.
             * CapacityDate eskiyse RemainingCapacity,
             * DailyCapacity değerine sıfırlanır.
             */
            EnsureCurrentDay(producerProfile);

            var orderTurkeyDate =
                _appClock.GetTurkeyDate(
                    orderCreatedAt);

            /*
             * Eski güne ait sipariş bugün iptal veya
             * reddedilirse bugünkü kapasite artırılmaz.
             */
            if (orderTurkeyDate !=
                _appClock.TurkeyToday)
            {
                return;
            }

            producerProfile.RemainingCapacity =
                Math.Min(
                    producerProfile.DailyCapacity,
                    producerProfile.RemainingCapacity
                    + quantity);
        }
    }
}