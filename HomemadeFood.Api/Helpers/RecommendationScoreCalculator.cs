namespace HomemadeFood.Api.Helpers
{
    public static class RecommendationScoreCalculator
    {
        private const double RatingWeight =
            0.40;

        private const double DistanceWeight =
            0.35;

        private const double PreparationWeight =
            0.25;

        public static double CalculateRatingScore(
            decimal averageRating,
            int reviewCount)
        {
            // Henüz yorumu bulunmayan yeni üreticiler
            // tamamen sıfır puanla cezalandırılmaz.
            if (reviewCount <= 0)
            {
                return 60;
            }

            var score =
                (double)averageRating * 20;

            return Math.Round(
                Math.Clamp(score, 0, 100),
                2);
        }

        public static double CalculateDistanceScore(
            double distanceKm)
        {
            var score =
                distanceKm switch
                {
                    <= 2 => 100,
                    <= 5 => 80,
                    <= 10 => 50,
                    <= 15 => 25,
                    _ => 0
                };

            return score;
        }

        public static double CalculatePreparationScore(
            int preparationTimeMinutes)
        {
            var score =
                preparationTimeMinutes switch
                {
                    <= 20 => 100,
                    <= 30 => 85,
                    <= 45 => 65,
                    <= 60 => 40,
                    _ => 20
                };

            return score;
        }

        public static double CalculateTotalScore(
            double ratingScore,
            double distanceScore,
            double preparationScore)
        {
            var totalScore =
                ratingScore * RatingWeight
                + distanceScore * DistanceWeight
                + preparationScore * PreparationWeight;

            return Math.Round(
                totalScore,
                2);
        }

        public static string CreateExplanation(
            double distanceKm,
            decimal averageRating,
            int reviewCount,
            int preparationTimeMinutes)
        {
            var reasons =
                new List<string>();

            if (distanceKm <= 2)
            {
                reasons.Add(
                    "müşteriye çok yakın olması");
            }
            else if (distanceKm <= 5)
            {
                reasons.Add(
                    "müşteriye yakın konumda olması");
            }

            if (reviewCount > 0 &&
                averageRating >= 4.5m)
            {
                reasons.Add(
                    "yüksek müşteri puanına sahip olması");
            }
            else if (reviewCount > 0 &&
                     averageRating >= 4.0m)
            {
                reasons.Add(
                    "iyi müşteri değerlendirmelerine sahip olması");
            }

            if (preparationTimeMinutes <= 30)
            {
                reasons.Add(
                    "hazırlama süresinin kısa olması");
            }

            if (reasons.Count == 0)
            {
                return
                    "Mevcut üreticiler arasında uygun seçeneklerden biri olduğu için önerildi.";
            }

            return
                string.Join(", ", reasons)
                + " nedeniyle önerildi.";
        }
    }
}