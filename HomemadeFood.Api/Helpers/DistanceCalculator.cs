namespace HomemadeFood.Api.Helpers
{
    public static class DistanceCalculator
    {
        private const double EarthRadiusKm =
            6371.0;

        public static double CalculateDistanceKm(
            double latitude1,
            double longitude1,
            double latitude2,
            double longitude2)
        {
            var latitudeDifference =
                DegreesToRadians(
                    latitude2 - latitude1);

            var longitudeDifference =
                DegreesToRadians(
                    longitude2 - longitude1);

            var firstLatitude =
                DegreesToRadians(latitude1);

            var secondLatitude =
                DegreesToRadians(latitude2);

            var haversine =
                Math.Pow(
                    Math.Sin(
                        latitudeDifference / 2),
                    2)
                +
                Math.Cos(firstLatitude)
                * Math.Cos(secondLatitude)
                * Math.Pow(
                    Math.Sin(
                        longitudeDifference / 2),
                    2);

            var angularDistance =
                2 * Math.Atan2(
                    Math.Sqrt(haversine),
                    Math.Sqrt(1 - haversine));

            var distance =
                EarthRadiusKm
                * angularDistance;

            return Math.Round(
                distance,
                2);
        }

        private static double DegreesToRadians(
            double degrees)
        {
            return degrees
                * Math.PI
                / 180.0;
        }
    }
}