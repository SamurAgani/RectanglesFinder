using System.Text;

namespace RectanglesFinder.Extentions
{
    public static class ExceptionExtension
    {
        public static StringBuilder GetInnerExceptions(this Exception e)
        {

            var messages = new StringBuilder();
            messages.Append("\n\n\n\n");
            do
            {
                messages.Append(e.Message + " \n");
                e = e.InnerException;
            }
            while (e != null);
            messages.Append("\n\n----------------------------------------------------------------------------------------------\n\n");
            return messages;
        }
    }
}
