<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Publish.aspx.cs" Inherits="GGApps.Publish" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="FeaturedContent" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
     <div class="sub">
        <ul id="menu">
            <li><a href="Configure.aspx">Configure</a></li>
            <li><a href="HomePage.aspx">Home Page</a></li>
            <li><a href="Promo.aspx">Promo</a></li>
            <li class="selected"><a href="Publish.aspx">Publish</a></li>
        </ul>
    </div>

    <asp:Panel ID="LoginViewImportant" CssClass="LoginViewImportant" runat="server">



         <h3>Select application to Publish</h3>
         <span>*All previous updates must have finished!</span>
            
        <br />
        <asp:DropDownList ID="SelectApp" CssClass="selectApp" runat="server" onselectedindexchanged="SelectApp_SelectedIndexChanged" AutoPostBack="true"></asp:DropDownList>
        <asp:ImageButton runat="server" ID="refersbtn" ImageUrl="~/Content/img/refreshDD.png" BorderStyle="None" BackColor="Transparent" OnClick="refreshDD_Click" style="width:26px; margin: 5px;"/>
        

        <h3>Just Produced</h3>
        <div style="width:960px;">
            <asp:ListView ID="latestVersions" runat="server"  ItemPlaceholderID="itemPlaceHolder1">
                <LayoutTemplate>
                    <table cellpadding="2" cellspacing="0" runat="server" border="1" class="latestVersions" style="width: 100%; height: 100px; border: dashed 2px #04AFEF; background-color: #B0E2F5; table-layout:fixed;">
                        <tr style="background-color: #E5E5FE">
                            <th style="width:50px;">Select</th>
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
                <EmptyItemTemplate>
                </EmptyItemTemplate>
                <EmptyDataTemplate>
                        <p>Nothing produced recently...</p>
                </EmptyDataTemplate>
            </asp:ListView>

        </div>
        <br />
        
        <h3>Version's Details</h3>

        <asp:Table ID="androidVerTable" runat="server" Width="100%" CssClass="latestVersions">
            <asp:TableHeaderRow>
                <asp:TableHeaderCell runat="server" ColumnSpan="4">Android</asp:TableHeaderCell>
            </asp:TableHeaderRow>
            <asp:TableHeaderRow>
                <asp:TableHeaderCell Wrap="true" Text="Environment"></asp:TableHeaderCell>
                <asp:TableHeaderCell Wrap="true" Text="Database Version"></asp:TableHeaderCell>
                <asp:TableHeaderCell Wrap="true" Text="Application Version"></asp:TableHeaderCell>
                <asp:TableHeaderCell Wrap="true" Text="Configuration Version"></asp:TableHeaderCell>
            </asp:TableHeaderRow>
           
            <asp:TableRow>
                <asp:TableCell ID="TableCell4" Text="Staging"></asp:TableCell>
                <asp:TableCell ID="stagAndDB"></asp:TableCell>
                <asp:TableCell ID="stagAndAV"></asp:TableCell>
                <asp:TableCell ID="stagAndCV"></asp:TableCell>
            </asp:TableRow>

            <asp:TableRow>
                <asp:TableCell ID="TableCell5" Text="Production"></asp:TableCell>
                <asp:TableCell ID="prodAndDB"></asp:TableCell>
                <asp:TableCell ID="prodAndAV"></asp:TableCell>
                <asp:TableCell ID="prodAndCV"></asp:TableCell>
            </asp:TableRow>
        </asp:Table>    
        
        <asp:Table ID="iosVerTable" runat="server" Width="100%" CssClass="latestVersions">
            <asp:TableHeaderRow>
                <asp:TableHeaderCell ID="TableHeaderCell1" runat="server" ColumnSpan="4">iOS</asp:TableHeaderCell>
            </asp:TableHeaderRow>
            <asp:TableHeaderRow>
                <asp:TableHeaderCell Wrap="true" Text="Environment"></asp:TableHeaderCell>
                <asp:TableHeaderCell Wrap="true" Text="Database Version"></asp:TableHeaderCell>
                <asp:TableHeaderCell Wrap="true" Text="Application Version"></asp:TableHeaderCell>
                <asp:TableHeaderCell Wrap="true" Text="Configuration Version"></asp:TableHeaderCell>
            </asp:TableHeaderRow>
            
            <asp:TableRow>
                <asp:TableCell ID="TableCell2" Text="Staging"></asp:TableCell>
                <asp:TableCell ID="stagIosDB"></asp:TableCell>
                <asp:TableCell ID="stagIosAV"></asp:TableCell>
                <asp:TableCell ID="stagIosCV"></asp:TableCell>
            </asp:TableRow>

            <asp:TableRow>
                <asp:TableCell ID="TableCell1" Text="Production"></asp:TableCell>
                <asp:TableCell ID="prodIosDB"></asp:TableCell>
                <asp:TableCell ID="prodIosAV"></asp:TableCell>
                <asp:TableCell ID="prodIosCV"></asp:TableCell>
            </asp:TableRow>

        </asp:Table>    

        <asp:Button runat="server" Text="Publish App to Production" ID="BtnPublishApp" ClientIDMode="Static" Enabled="false" OnClick="BtnPublishApp_Click" CssClass="InputDisabledCustom"/>
        <asp:Button runat="server" Text="Undo Publish" ID="undoPublish" Enabled="false" CssClass="InputDisabledCustom"/>

        <asp:CustomValidator runat="server" id="cusCustom" onservervalidate="cusCustom_ServerValidate" errormessage="Custom Error Message!" />

    </asp:Panel>
</asp:Content>
