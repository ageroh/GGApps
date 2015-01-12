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
    <asp:Panel ID="LoginViewImportant" CssClass="LoginViewImportant" runat="server">

         <h3>Select application to Publish</h3>
         <span>*All previous updates must have finished!</span>
            
        <br />
        <asp:DropDownList ID="SelectApp" Width="200" runat="server" onselectedindexchanged="SelectApp_SelectedIndexChanged" AutoPostBack="true"></asp:DropDownList>
            
        <h3>Just Produced</h3>
        <div style="width:100%">
            <asp:ListView ID="latestVersions" runat="server" ItemPlaceholderID="itemPlaceHolder1">
                <LayoutTemplate>
                    <table cellpadding="2" cellspacing="0" runat="server"  border="1" style="width: 200px; height: 100px; border: dashed 2px #04AFEF; background-color: #B0E2F5; table-layout:auto;">
                        <tr style="background-color: #E5E5FE">
                            <th>Select</th>
                            <th>Mobile Device</th>
                            <th>Database Version</th>
                            <th>Application Version</th>
                            <th>Configuration Version</th>
                            <th>Date Produced</th>
                        </tr>
                        <tr id="itemPlaceHolder1" runat="server"></tr>
                    </table>
                </LayoutTemplate>
                <ItemTemplate>
                    <tr>
                        <td>
                            <asp:CheckBox ID="chkSelected" runat="server" />
                        </td>
                        <td>
                            <asp:TextBox ID="txtmobileDevice" Text='<%#Eval("mobileDevice") %>' Enabled="false" Font-Bold="true" runat="server"></asp:TextBox>
                        </td>
                        <td>
                            <asp:TextBox ID="txtdbversion" Text='<%#Eval("db_version") %>'  Enabled="false" runat="server"></asp:TextBox>
                        </td>
                        <td>
                            <asp:TextBox ID="txtappversion" Text='<%#Eval("app_version") %>'  Enabled="false" runat="server"></asp:TextBox>
                        </td>
                        <td>
                            <asp:TextBox ID="txtconfigversion" Text='<%#Eval("config_version") %>'  Enabled="false" runat="server"></asp:TextBox>
                        </td>

                        <td>
                            <asp:TextBox ID="txtDateProduced" Text='<%#Eval("DateProduced") %>'  Enabled="false" runat="server"></asp:TextBox>
                        </td>

                    </tr>
                    <tr id="itemPlaceHolder1" runat="server"></tr>
                </ItemTemplate>
            </asp:ListView>

        </div>
        <br />
        
        <h3>Production Now</h3>

        <div style="width:100%">
            <asp:ListView ID="VersionProduction" runat="server" ItemPlaceholderID="itemPlaceHolder1">
                <LayoutTemplate>
                    <table id="Table1" cellpadding="2" cellspacing="0" runat="server"  border="1" style="width: 200px; height: 100px; border: dashed 2px #04AFEF; background-color: #B0E2F5">
                        <tr style="background-color: #E5E5FE">
                            <th>Mobile Device</th>
                            <th>Database Version</th>
                            <th>Application Version</th>
                            <th>Configuration Version</th>
                        </tr>
                        <tr id="itemPlaceHolder1" runat="server"></tr>
                    </table>
                </LayoutTemplate>
                <ItemTemplate>
                    <tr>
                        <td>
                            <asp:TextBox ID="txtmobileDevice" Text='<%#Eval("mobileDevice") %>' Enabled="false" Font-Bold="true" runat="server"></asp:TextBox>
                        </td>
                        <td>
                            <asp:TextBox ID="txtdbversion" Text='<%#Eval("db_version") %>'  Enabled="false" runat="server"></asp:TextBox>
                        </td>
                        <td>
                            <asp:TextBox ID="txtappversion" Text='<%#Eval("app_version") %>'  Enabled="false" runat="server"></asp:TextBox>
                        </td>
                        <td>
                            <asp:TextBox ID="txtconfigversion" Text='<%#Eval("config_version") %>'  Enabled="false" runat="server"></asp:TextBox>
                        </td>
                    </tr>
                    <tr id="itemPlaceHolder1" runat="server"></tr>
                </ItemTemplate>
            </asp:ListView>
        </div>

        <asp:Button runat="server" Text="Publish App to Production" ID="publishApp"/>
        <asp:Button runat="server" Text="Undo Publish" ID="undoPublish" />
    </asp:Panel>
</asp:Content>
