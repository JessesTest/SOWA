using Microsoft.EntityFrameworkCore;
using SW.DAL.Contexts;
using SW.DM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace SW.BillEmailer.Services;

public static class SwDbContextExtensions
{
    #region Customers

    public static Task<List<Customer>> GetAllCustomers(this SwDbContext db)
    {
        return db.Customers.ToListAsync();
    }

    #endregion

    #region Bill Blobs

    public static Task<BillBlobs> GetBillBlobByAddDateTimeRange(this SwDbContext db, int customer_id, DateTime beg_datetime, DateTime end_datetime)
    {
        return db.BillBlobs.Where(b => !b.DelFlag && b.CustomerId == customer_id && b.BegDateTime >= beg_datetime && b.EndDateTime <= end_datetime).SingleOrDefaultAsync();
    }

    #endregion
}