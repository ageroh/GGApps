<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="HomePage.aspx.cs" Inherits="GGApps.HomePage" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="FeaturedContent" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
        <div class="sub">
        <ul id="menu">
            <li><a href="Configure.aspx">Configure</a></li>
            <li class="selected"><a href="HomePage.aspx">Home Page</a></li>
            <li><a href="Promo.aspx">Promo</a></li>
            <li><a href="Publish.aspx">Publish</a></li>
        </ul>
    </div>
</asp:Content>
