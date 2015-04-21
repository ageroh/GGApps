<%@ Page Title="Admin" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Admin.aspx.cs" Inherits="GGApps.Admin" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="FeaturedContent" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <div class="sub">
        <ul id="menu">
            <li><a href="Admin/Configure.aspx">Configure</a></li>
            <li><a href="Admin/HomePage.aspx">Home Page</a></li>
            <li><a href="Admin/Promo.aspx">Promo</a></li>
            <li><a href="Admin/Publish.aspx">Publish</a></li>
        </ul>
    </div>
</asp:Content>
