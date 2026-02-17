namespace SingularityGroup.HotReload.Editor.Localization {
    internal static partial class Translations {
        public static class Registration {
            // Registration & Redeem
            public static string MessageRegistrationProUsers;
            public static string MessageRegistrationLicensingModel;
            public static string MessageRedeemInstructions;
            public static string MessageRedeemSuccess;
            public static string MessageRedeemAlreadyClaimed;
            public static string UnknownRedeemError;
            
            public static void LoadEnglish() {
                // Registration & Redeem
                MessageRegistrationProUsers = "Unity Pro users are required to obtain an additional license. You are eligible to redeem one if your company has ten or fewer employees. Please enter your company details below.";
                MessageRegistrationLicensingModel = "The licensing model for Unity Pro users varies depending on the number of employees in your company. Please enter your company details below.";
                MessageRedeemInstructions = "To enable us to verify your purchase, please enter your invoice number/order ID. Additionally, provide the email address that you intend to use for managing your credentials.";
                MessageRedeemSuccess = "Success! You will receive an email containing your license password shortly. Once you receive it, please enter the received password in the designated field below to complete your registration.";
                MessageRedeemAlreadyClaimed = "Your license has already been redeemed. Please enter your existing password below.";
                UnknownRedeemError = "We apologize, an error happened while redeeming your license. Please reach out to customer support for assistance.";
            }
            
            public static void LoadSimplifiedChinese() {
                // Registration & Redeem
                MessageRegistrationProUsers = "Unity Pro 用户需要获得额外许可证。如果您的公司有十名或更少员工，您有资格兑换一个。请在下方输入您的公司详细信息。";
                MessageRegistrationLicensingModel = "Unity Pro 用户的许可模式因公司员工人数而异。请在下方输入您的公司详细信息。";
                MessageRedeemInstructions = "为使我们能够验证您的购买，请输入您的发票号码/订单 ID。此外，请提供您打算用于管理凭据的电子邮件地址。";
                MessageRedeemSuccess = "成功！您很快将收到一封包含许可证密码的电子邮件。收到后，请在下方指定字段中输入收到的密码以完成注册。";
                MessageRedeemAlreadyClaimed = "您的许可证已被兑换。请在下方输入您现有的密码。";
                UnknownRedeemError = "我们深表歉意，兑换您的许可证时发生错误。请联系客户支持寻求帮助。";
            }
        }
    }
}

