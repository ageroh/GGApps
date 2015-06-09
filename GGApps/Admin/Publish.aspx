<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Publish.aspx.cs" Inherits="GGApps.Publish" %>


<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="FeaturedContent" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">

    <script type="text/javascript">
        var refreshIntervalId;
    </script>
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
        <asp:DropDownList ID="SelectApp" CssClass="selectApp" runat="server" OnSelectedIndexChanged="SelectApp_SelectedIndexChanged" AutoPostBack="true"></asp:DropDownList>
        <asp:ImageButton runat="server" ID="refersbtn" ImageUrl="~/Content/img/refreshDD.png" BorderStyle="None" BackColor="Transparent" OnClick="refreshDD_Click" Style="width: 26px; margin: 5px;" />


        <h3>Just Produced(Staging)</h3>
        <div style="width: 960px;">
            <asp:ListView ID="latestVersions" runat="server" ItemPlaceholderID="itemPlaceHolder1">
                <LayoutTemplate>
                    <table cellpadding="2" cellspacing="0" runat="server" border="1" class="latestVersions" style="width: 100%; height: 100px; border: dashed 2px #04AFEF; background-color: #B0E2F5; table-layout: fixed;">
                        <tr style="background-color: #E5E5FE">
                            <th style="width: 50px;">What To Publish</th>
                            <th style="width: 50px;">App Update?</th>
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
                            <asp:CheckBox ID="chkSelectedAppUpdate" Enabled="false"  runat="server" />
                        </td>
                        <td>
                            <asp:TextBox ID="txtmobileDevice" Text='<%#Eval("mobileDevice") %>' Enabled="false" Font-Bold="true" runat="server"></asp:TextBox>
                        </td>
                        <td>
                            <asp:TextBox ID="txtdbversion" Text='<%#Eval("db_version") %>' Enabled="false" runat="server"></asp:TextBox>
                        </td>
                        <td>
                            <asp:TextBox ID="txtappversion" Text='<%#Eval("app_version") %>' Enabled="false" runat="server"></asp:TextBox>
                        </td>
                        <td>
                            <asp:TextBox ID="txtconfigversion" Text='<%#Eval("config_version") %>' Enabled="false" runat="server"></asp:TextBox>
                        </td>

                        <td>
                            <asp:TextBox ID="txtDateProduced" Text='<%#Eval("DateProduced") %>' Enabled="false" runat="server"></asp:TextBox>
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
                <asp:TableHeaderCell runat="server" ColumnSpan="5">Android</asp:TableHeaderCell>
            </asp:TableHeaderRow>
            <asp:TableHeaderRow>
                <asp:TableHeaderCell Wrap="true" Text="Environment"></asp:TableHeaderCell>
                <asp:TableHeaderCell Wrap="true" Text="Location"></asp:TableHeaderCell>
                <asp:TableHeaderCell Wrap="true" Text="Database Version"></asp:TableHeaderCell>
                <asp:TableHeaderCell Wrap="true" Text="Application Version"></asp:TableHeaderCell>
                <asp:TableHeaderCell Wrap="true" Text="Configuration Version"></asp:TableHeaderCell>
            </asp:TableHeaderRow>

            <asp:TableRow>
                <asp:TableCell ID="TableCell4" Text="Staging"></asp:TableCell>
                <asp:TableCell ID="stagAndName"></asp:TableCell>
                <asp:TableCell ID="stagAndDB"></asp:TableCell>
                <asp:TableCell ID="stagAndAV"></asp:TableCell>
                <asp:TableCell ID="stagAndCV"></asp:TableCell>
                <asp:TableCell ID="stagAndLIVE" CssClass="onoffApp">
                   <asp:Button id="stagAndLIVEonoff" runat="server" CssClass="online" Text="On-Line" BackColor="Green" ForeColor="White" Enabled="false" Visible="false" OnClick="stagAndLIVEonoff_Click"/>
                   
                </asp:TableCell>
            </asp:TableRow>

            <asp:TableRow>
                <asp:TableCell ID="TableCell5" Text="Production"></asp:TableCell>
                <asp:TableCell ID="prodAndName"></asp:TableCell>
                <asp:TableCell ID="prodAndDB"></asp:TableCell>
                <asp:TableCell ID="prodAndAV"></asp:TableCell>
                <asp:TableCell ID="prodAndCV"></asp:TableCell>
                <asp:TableCell ID="prodAndLIVE" CssClass="onoffApp">
                   <asp:Button id="prodAndLIVEEonoff" runat="server" CssClass="online" Text="On-Line" BackColor="Green" ForeColor="White" Enabled="false" Visible="false" OnClick="prodAndLIVEEonoff_Click"/>
                </asp:TableCell>
            </asp:TableRow>
        </asp:Table>

        <asp:Table ID="iosVerTable" runat="server" Width="100%" CssClass="latestVersions">
            <asp:TableHeaderRow>
                <asp:TableHeaderCell ID="TableHeaderCell1" runat="server" ColumnSpan="5">iOS</asp:TableHeaderCell>
            </asp:TableHeaderRow>
            <asp:TableHeaderRow>
                <asp:TableHeaderCell Wrap="true" Text="Environment"></asp:TableHeaderCell>
                <asp:TableHeaderCell Wrap="true" Text="Location"></asp:TableHeaderCell>
                <asp:TableHeaderCell Wrap="true" Text="Database Version"></asp:TableHeaderCell>
                <asp:TableHeaderCell Wrap="true" Text="Application Version"></asp:TableHeaderCell>
                <asp:TableHeaderCell Wrap="true" Text="Configuration Version"></asp:TableHeaderCell>
            </asp:TableHeaderRow>

            <asp:TableRow>
                <asp:TableCell ID="TableCell2" Text="Staging"></asp:TableCell>
                <asp:TableCell ID="stagIosName"></asp:TableCell>
                <asp:TableCell ID="stagIosDB"></asp:TableCell>
                <asp:TableCell ID="stagIosAV"></asp:TableCell>
                <asp:TableCell ID="stagIosCV"></asp:TableCell>
                <asp:TableCell ID="stagIosLIVE"  CssClass="onoffApp">
                    <asp:Button id="stagIosLIVEonoff" runat="server" CssClass="online" Text="On-Line" BackColor="Green" ForeColor="White" Enabled="false" Visible="false" OnClick="stagIosLIVEonoff_Click"/>
                </asp:TableCell>
            </asp:TableRow>

            <asp:TableRow>
                <asp:TableCell ID="TableCell1" Text="Production"></asp:TableCell>
                <asp:TableCell ID="prodIosName"></asp:TableCell>
                <asp:TableCell ID="prodIosDB"></asp:TableCell>
                <asp:TableCell ID="prodIosAV"></asp:TableCell>
                <asp:TableCell ID="prodIosCV"></asp:TableCell>
                <asp:TableCell ID="prodIosLIVE"  CssClass="onoffApp">
                    <asp:Button id="prodIosLIVEonoff" runat="server" CssClass="online" Text="On-Line" BackColor="Green" ForeColor="White" Enabled="false" Visible="false" OnClick="prodIosLIVEonoff_Click"/>
                </asp:TableCell>
            </asp:TableRow>

        </asp:Table>

        <asp:Button runat="server" Text="Publish App to Production" ID="BtnPublishApp" ClientIDMode="Static" Enabled="false" OnClick="BtnPublishApp_Click" CssClass="InputDisabledCustom" />
        <asp:Button runat="server" Text="Undo Publish" ID="undoPublishBtn" ClientIDMode="Static" Enabled="false" CssClass="InputDisabledCustom" OnClick="undoPublish_Click" />

        <asp:Label CssClass="ErrorGeneral" runat="server" ID="custValidation" Text="" Visible="false"></asp:Label>


        
        <div id="openModal" class="modalDialogSuccess">
	        <div>
		        <a href="#close" title="Close" class="closeSuccess">X</a>
		        <h2>Successfull Publish!</h2>
		        <div id="txtMessageModal" runat="server">
		        </div>
	        </div>
        </div>

    </asp:Panel>
</asp:Content>
