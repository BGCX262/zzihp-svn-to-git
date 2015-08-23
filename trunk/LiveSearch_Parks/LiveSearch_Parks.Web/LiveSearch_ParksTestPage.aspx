<%@ Page Language="c#" AutoEventWireup="true" %>
<%@ Import Namespace="System.Web.UI.SilverlightControls" %>
<%@ Import Namespace="LiveSearch_Parks.Web.net.virtualearth.common.staging" %>
<%@ Register Assembly="System.Web.Silverlight" Namespace="System.Web.UI.SilverlightControls"
    TagPrefix="asp" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<script runat="server">

    protected void Page_Load(object sender, EventArgs e)
    {
        // set up the login credentials for Live Search API and Virtual Earth
        // it is best to limit exposure of these on client side!
        string MSNSearchAppID = "2E08617E619A56E62BDB7525108FCD110553A819";
        string virtualEarthAccountID = "136035";
        string virtualEarthPassword = "V1rtual!";


        CommonService commonService = new CommonService();
        commonService.Url =
          "https://staging.common.virtualearth.net/find-30/common.asmx";
        commonService.Credentials = new System.Net.NetworkCredential(virtualEarthAccountID,
          virtualEarthPassword);

        // Create the TokenSpecification object to pass to GetClientToken.
        TokenSpecification tokenSpec = new TokenSpecification();

        // Use the ‘Page’ object to retrieve the end-client’s IPAddress.
        tokenSpec.ClientIPAddress = Page.Request.UserHostAddress;

        // The maximum allowable token duration is 480 minutes (8 hours).
        // The minimum allowable duration is 15 minutes.
        tokenSpec.TokenValidityDurationMinutes = 480;

        // Now get a token from the Virtual Earth Platform service 
        string veToken = commonService.GetClientToken(tokenSpec);
        // and pass it to the Silverlight control
        System.Web.UI.SilverlightControls.Silverlight Xaml1 = (System.Web.UI.SilverlightControls.Silverlight)this.FindControl("Xaml1");
        Xaml1.InitParameters = "clientToken=" + veToken + ",MSNSearchAppID=" + MSNSearchAppID;

    }
    
    
    </script>
<html xmlns="http://www.w3.org/1999/xhtml" style="height:100%;">
<head runat="server">
    <title>Global Electronics - Parks &amp; Recreation Search</title>
    <link href="global.css" rel="stylesheet" type="text/css">
</style>
</head>
<body style="height:100%;margin:0;">
    
    <div id="container">
<div id="LiveSearch_App">
<form id="form1" runat="server" style="height:100%;">
        <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
        <div  style="height:100%;">
            <asp:Silverlight ID="Xaml1" runat="server" Source="~/ClientBin/LiveSearch_Parks.xap" MinimumVersion="2.0.31005.0" Width="100%" Height="100%" />
        </div>
    </form>
</div>
	<div id="sidebar">
			<div id="sidebar_top" style="height: 63px">
   			 Global Electronics<br />
				Healthpage</div>
			<p class="MsoNormal">
			<span style="color: #008000; font-size: x-large; font-weight: normal">
			Get Healthy</span></p>
			<p class="MsoNormal">
			<span style="color: #008000; font-size: x-large; font-weight: bold">
			Enjoy the Outdoors</span></p>
			<p class="MsoNormal">
			<span style="color: #000000">At Global Electronics we 
			want employees, and visitors, at our corporate offices to enjoy the 
			great outdoors. </span></p>
			<p class="MsoNormal">
			<span style="color: #000000">We have created the California Park Finder to locate 
			the outdoor activities that meet your needs and fit your lifestyle.</span></p>
			<p class="MsoListParagraph" style="text-indent: -.25in;  margin-left: 0.25in;">
			
			<span style="  color: #000000">
			<span style="">1.<span style="font: 7.0pt;">&nbsp;&nbsp;&nbsp;
			</span></span></span><span style="color: #000000">Click 
			the icons that represents the activities you would like to locate.</span></p>
			<p class="MsoListParagraph" style="text-indent: -.25in;  margin-left: 0.25in;">
			
			<span style="  color: #000000">
			<span style="">2.<span style="font: 7.0pt;">&nbsp;&nbsp;&nbsp;
			</span></span></span><span style="color: #000000">Press 
			the Go button</span></p>
			<p class="MsoListParagraph" style="text-indent: -.25in;  margin-left: 0.25in; color: #000000">
			
			<span style="  color: #000000">
			<span style="">3.<span style="font: 7.0pt;">&nbsp;&nbsp;&nbsp;
			</span></span></span><span style="color: #000000">Use the 
			results and map to find a great location to enjoy!</span></p>
		</div>
</div>
    
</body>
</html>