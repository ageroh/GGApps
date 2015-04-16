<%@ Page Title="Content Validation" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ContentValidation.aspx.cs" Inherits="GGApps.ContentValidation" %>

<asp:Content runat="server" ID="FeaturedContent" ContentPlaceHolderID="FeaturedContent">
        <section class="featured">
                <div class="content-wrapper">
                    <hgroup class="title">
                        <h2>Select Destination, Language and Period to Validate all Entities</h2>
                    </hgroup>
                </div>
            </section>
</asp:Content>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <asp:Panel ID="LoginViewImportant" CssClass="LoginViewImportant" runat="server">
        <table>
            <tr>
                <th>Destination</th>
                <th>Language</th>
                <th>Time Period</th>
            </tr>
            <tr>
                <td>
                    <asp:DropDownList ID="ddStart" CssClass="selectApp" runat="server"></asp:DropDownList>
                </td>
                <td>
                    <asp:DropDownList ID="ddLang" CssClass="selectApp" runat="server">
                        <asp:ListItem Text="Greek" Value="1"></asp:ListItem>
                        <asp:ListItem Text="English" Value="2"></asp:ListItem>
                        <asp:ListItem Text="Russian" Value="4"></asp:ListItem>
                    </asp:DropDownList>
                </td>
                <td>
                    <asp:DropDownList ID="ddTimePeriod" CssClass="selectApp" runat="server">
                        <asp:ListItem Text="1 Month" Value="1"></asp:ListItem>
                        <asp:ListItem Text="3 Months" Value="3"></asp:ListItem>
                        <asp:ListItem Text="6 Months" Value="6"></asp:ListItem>
                    </asp:DropDownList>
                </td>
            </tr>
            <tr>
                <td colspan="3">
                    <asp:Button id="Goload" runat="server" Text="GO!" OnClick="Goload_Click" Font-Size="X-Large" ForeColor="#ff0000"/>
                </td>
            </tr>
        </table>
        

        
        <div id="entityPlaceHolder" runat="server">
        </div>        
        <asp:Button id="GetNextEntity" runat="server" Text=" OK -> " Visible="false" OnClick="GetNextEntity_Click"/>
        
        <asp:Button id="RefreshBtn" runat="server" Text="Restart" Visible="false" />

    </asp:Panel>
</asp:Content>


