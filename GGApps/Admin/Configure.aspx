<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Configure.aspx.cs" Inherits="GGApps.Configure" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="FeaturedContent" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">

      <div class="sub">
        <ul id="menu">
            <li class="selected"><a href="/Admin/Configure.aspx">Configure</a></li>
            <li><a href="/Admin/HomePage.aspx">Home Page</a></li>
            <li><a href="/Admin/Promo.aspx">Promo</a></li>
            <li><a href="/Admin/Publish.aspx">Publish</a></li>
        </ul>
    </div>
     <asp:Panel runat="server" ID="mainAdminPanel">

        <asp:RadioButtonList ID="EnvironmentConfig" CssClass="BuildAppList" runat="server" OnSelectedIndexChanged="EnvironmentConfig_SelectedIndexChanged" >
            <asp:ListItem
                    Enabled="True"
                    Selected="true"
                    Text="Staging"
                    Value="Staging"
                />
            <asp:ListItem
                    Enabled="True"
                    Selected="false"
                    Text="Production"
                    Value="Production"
                />
        </asp:RadioButtonList>


         <h3>Select application to Configure</h3>
         <span>*All previous updates must have finished!</span>
            
        <br />
        <asp:DropDownList ID="SelectApp" Width="200" runat="server"></asp:DropDownList>

        <div style="width:100%">
        <asp:ListView runat="server" ID="latestVersions"></asp:ListView>
            </div>
        <asp:Button runat="server" Text="Publish App to Production" ID="publishApp"/>
        <asp:Button runat="server" Text="Undo Publish" ID="undoPublish" />
    </asp:Panel>
</asp:Content>
