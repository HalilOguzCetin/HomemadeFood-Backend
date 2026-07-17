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

            // Kapasite bilgileri değiştiği için
            // concurrency sürümünü artır.
            producerProfile.CapacityVersion++;
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

            // Sipariş için kapasite azaltıldı.
            producerProfile.CapacityVersion++;

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

            EnsureCurrentDay(producerProfile);

            var orderTurkeyDate =
                _appClock.GetTurkeyDate(
                    orderCreatedAt);

            // Eski güne ait sipariş bugünün
            // kapasitesini artırmamalı.
            if (orderTurkeyDate !=
                _appClock.TurkeyToday)
            {
                return;
            }

            var oldRemainingCapacity =
                producerProfile.RemainingCapacity;

            producerProfile.RemainingCapacity =
                Math.Min(
                    producerProfile.DailyCapacity,
                    producerProfile.RemainingCapacity
                    + quantity);

            // Gerçekten kapasite değiştiyse
            // concurrency sürümünü artır.
            if (producerProfile.RemainingCapacity !=
                oldRemainingCapacity)
            {
                producerProfile.CapacityVersion++;
            }
        }
    }
}