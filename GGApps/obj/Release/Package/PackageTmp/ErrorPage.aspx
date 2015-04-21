<%@ Page Title="Content Validation" Language="C#" MasterPageFile="~/Validation.Master" AutoEventWireup="true" CodeBehind="ErrorPage.aspx.cs" Inherits="GGApps.ErrorPage" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <asp:Panel ID="LoginViewImportant" CssClass="LoginViewImportant" runat="server">
        <h1>
            Some error occured!
        </h1>
    </asp:Panel>
</asp:Content>

