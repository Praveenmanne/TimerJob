using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint;

namespace TimerJob
{
    public class NotificationTimerJob : SPJobDefinition
    {

        public NotificationTimerJob()
        {
        }

        public NotificationTimerJob(string jobName, SPService service, SPServer server, SPJobLockType lockType)
            : base(jobName, service, server, lockType)
        {
            Title = jobName;
        }

        public NotificationTimerJob(string jobName, SPWebApplication webapp)
            : base(jobName, webapp, null, SPJobLockType.ContentDatabase)
        {
            Title = jobName;
        }

        public override void Execute(Guid targetInstanceId)
        {
            var webapp = Parent as SPWebApplication;
            if (webapp != null)
            {
                foreach (SPSite spSite in webapp.Sites)
                {
                    using (var web = spSite.OpenWeb())
                    {
                        spSite.RootWeb.Lists.TryGetList(Utilities.EmployeeLeaves);

                        SPListItemCollection employeetype = GetListItemCollection(web.Lists[Utilities.EmployeeLeaves], "Employee Type", "Probationary");

                        foreach (SPListItem currentUseremptypeDetail in employeetype)
                        {
                            currentUseremptypeDetail[Utilities.LeaveBalancecolname] =
                                Convert.ToInt16(currentUseremptypeDetail[Utilities.LeaveBalancecolname]) + 1;
                            currentUseremptypeDetail.Update();
                        }
                    }
                }
            }
        }

        internal SPListItemCollection GetListItemCollection(SPList spList, string key, string value)
        {
            // Return list item collection based on the lookup field

            SPField spField = spList.Fields[key];
            var query = new SPQuery
            {
                Query = @"<Where>
                        <Eq>
                            <FieldRef Name='" + spField.InternalName + @"'/><Value Type='" + spField.Type.ToString() + @"'>" + value + @"</Value>
                        </Eq>
                        </Where>"
            };

            return spList.GetItems(query);
        }


    }
}
