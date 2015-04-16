<%@ Page Title="Content Validation" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Validation.aspx.cs" Inherits="Extras.Validation" %>

<asp:Content runat="server" ID="FeaturedContent" ContentPlaceHolderID="FeaturedContent">
        <section class="featured">
                <div class="content-wrapper">
                    <hgroup class="title">
                        <h2>Select the App to update for GREEKGUIDE.COM</h2>
                    </hgroup>
                </div>
            </section>
</asp:Content>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <asp:Panel ID="LoginViewImportant" CssClass="LoginViewImportant" runat="server">
            <h3>Select application to update</h3>
            <span>*All previous updates must have finished!</span>
            
            <br />
            <asp:DropDownList ID="ddStart" CssClass="selectApp" runat="server"></asp:DropDownList>
            <asp:Button id="GoFirst" runat="server" Text="Build Report" OnClick="GoFirst_Click"/>


            
            <textarea readonly id="MainTextArea" rows="400" cols="80"  style="height:200px; display:none;" >
                <asp:Literal runat="server" id="txtEditor1" />
            </textarea>

            <div class="reportDivClass" runat="server" id="reportDiv" >
            </div>
            </br>
            <asp:Button id="ContinueBtn" runat="server" Text="Continue" Visible="false" Enabled="false" OnClick="ContinueBtn_Click"/>                   

    </asp:Panel>
</asp:Content>
