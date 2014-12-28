<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Publish.aspx.cs" Inherits="GGApps.Publish" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="FeaturedContent" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
     <div class="sub">
        <ul id="menu">
            <li><a href="/Admin/Configure.aspx">Configure</a></li>
            <li><a href="/Admin/HomePage.aspx">Home Page</a></li>
            <li><a href="/Admin/Promo.aspx">Promo</a></li>
            <li class="selected"><a href="/Admin/Publish.aspx">Publish</a></li>
        </ul>
    </div>
    <asp:Panel>


         <h3>Select application to Publish</h3>
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
