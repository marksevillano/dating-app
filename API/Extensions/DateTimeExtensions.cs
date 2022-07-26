namespace API.Extensions
{
    public static class DateTimeExtensions
    {
        public static int CalculateAge(this DateTime dob)
        {
            var dateToday = DateTime.Now;
            var age = dateToday.Year - dob.Year;
            if (dob.Date > dateToday.AddYears(-age)) age--;
            return age;
        }
    }
}