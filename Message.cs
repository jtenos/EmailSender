namespace EmailSender
{
    record Message(string SenderName, string SenderEmail, Recipient[] Recipients,
        string Subject, string HtmlBody, string TextBody);
}
