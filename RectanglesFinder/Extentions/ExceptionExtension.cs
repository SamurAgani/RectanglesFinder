namespace RectanglesFinder.Extentions
{
    public static class ExceptionExtension
    {
        public static string GetInnerExceptions(this Exception e)
        {

            string messages = string.Empty;
            messages += "\n\n\n\n";
            do
            {
                messages += e.Message + " \n";
                e = e.InnerException;
            }
            while (e != null);
            messages += "\n\n----------------------------------------------------------------------------------------------\n\n";
            return messages;
        }
    }
}
