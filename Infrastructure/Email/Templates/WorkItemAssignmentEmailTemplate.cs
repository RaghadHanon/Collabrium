using Application.Interfaces.Emails;

namespace Infrastructure.Email.Templates;

public class WorkItemAssignmentEmailTemplate : IEmailTemplate
{
    public string GetSubject()
    {
        return "Task Assignment: A new Work Item has been assigned to you";
    }

    public string GetPlainTextBody(Dictionary<string, string> data)
    {
        var assigneeFullName = data["AssigneeFullName"];
        var assignerFullName = data["AssignerFullName"];
        var workItemCode = data["WorkItemCode"];
        var workItemTitle = data["WorkItemTitle"];
        var workItemType = data["WorkItemType"];
        var url = data["Url"];

        return $@"Hi {assigneeFullName},

A new work item has been assigned to you by {assignerFullName}.

Details:
- Code: {workItemCode}
- Title: {workItemTitle}
- Type: {workItemType}

You can view the details here: {url}

Regards,
Collabrium Team";
    }

    public string GetHtmlBody(Dictionary<string, string> data)
    {
        var assigneeFullName = data["AssigneeFullName"];
        var assignerFullName = data["AssignerFullName"];
        var workItemCode = data["WorkItemCode"];
        var workItemTitle = data["WorkItemTitle"];
        var workItemType = data["WorkItemType"];
        var workItemPriority = data["WorkItemPriority"];
        var url = data["Url"];

        return $@"
<html>
  <body style=""font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f4f7f6; padding: 20px;"">
    <table width=""100%"" cellpadding=""0"" cellspacing=""0"" 
           style=""max-width: 600px; margin: auto; background: #ffffff; border-radius: 12px; 
                  box-shadow: 0 4px 12px rgba(0,0,0,0.05); overflow: hidden;"">
      <tr>
        <td style=""text-align: center; padding: 20px 0px"">
           <img src=""https://res.cloudinary.com/dcfpagr5v/image/upload/v1772752623/Collabrium/ProfileImages/mqogyk5po3aoq7gv2oov.jpg"" 
                alt=""Collabrium Logo"" style=""display: block; margin: 0 auto; width: 150px; height: 150px; 
                         border-radius: 50%; object-fit: cover; -webkit-border-radius: 50%; -moz-border-radius: 50%;"" />
          <h2 style=""color: #2a3f58; margin-top: 10;"">New Work Item Assigned</h2>
        </td>
      </tr>
      
      <tr>
        <td style=""padding: 40px 30px;"">
          <p style=""color: #555; font-size: 16px; line-height: 1.6;"">
            Hi <strong>{assigneeFullName}</strong>,<br/><br/>
            You have been assigned a new work item by <strong>{assignerFullName}</strong>. 
            Below are the details of your task:
          </p>
          
          <div style=""background: #f9f9f9; border-left: 4px solid #ECBF28; padding: 15px; margin: 25px 0;"">
            <p style=""margin: 5px 0; color: #333;""><strong>Code:</strong> #{workItemCode}</p>
            <p style=""margin: 5px 0; color: #333;""><strong>Type:</strong> {workItemType}</p>
            <p style=""margin: 5px 0; color: #333;""><strong>Title:</strong> {workItemTitle}</p>
            <p style=""margin: 5px 0; color: #333;""><strong>Priority:</strong> {workItemPriority}</p>
          </div>

          <p style=""text-align: center; margin: 35px 0;"">
            <a href=""{url}"" 
               style=""background: #ECBF28; color: #fff; text-decoration: none; 
                      padding: 14px 30px; border-radius: 6px; font-weight: bold; display: inline-block;"">
              View Work Item
            </a>
          </p>
          
          <hr style=""border: none; border-top: 1px solid #eee; margin: 30px 0;"" />
          <p style=""color: #888; font-size: 13px; text-align: center;"">
            This is an automated notification from Collabrium. 
            Please do not reply directly to this email.
          </p>
        </td>
      </tr>
    </table>
  </body>
</html>";
    }
}