using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MimeKit;

namespace MailSync.Core.Services
{
    public class DateFixService
    {
        //private readonly MailContext _context;
        
        //public DateFixService(MailContext context)
        //{
        //    _context = context;
        //}

        //public async Task FixForAccount(int accountId)
        //{
        //    _context.Accounts.First(a => a.Id == accountId);
        //}

        public static bool FixDate(MimeMessage message)
        {
            var mimeDate = GetDateFromMime(message);
            
            if (mimeDate == null)
                return false;

            if (mimeDate.Value.AddHours(12) >= message.Date)
                return false;

            message.Date = mimeDate.Value;
            return true;
        }

        private static DateTime? GetDateFromMime(MimeMessage message)
        {
            var dateHeader = message.Headers.FirstOrDefault(x => x.Field == "Date");
            
            if (dateHeader != null && DateTime.TryParse(dateHeader.Value, out var date))
                return date;

            dateHeader = message.Headers.FirstOrDefault(x => x.Field == "Received");
            
            if (dateHeader == null)
                return null;
            
            if (!dateHeader.Value.Contains(";") && DateTime.TryParse(dateHeader.Value, out date))
                return date;

            if (DateTime.TryParse(dateHeader.Value.Split(';')[1], out date))
                return date;

            return null;
        }
    }
}
