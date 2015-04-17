<%@ Page Title="Content Validation" Language="C#" MasterPageFile="~/Validation.Master" AutoEventWireup="true" CodeBehind="ContentValidation.aspx.cs" Inherits="GGApps.ContentValidation" %>


<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <style type="text/css">
        #<%= entityPlaceHolder.ClientID%> span {font-size:larger;}
    </style>
    <asp:Panel ID="LoginViewImportant" CssClass="LoginViewImportant" runat="server">
        <table id="destLangTimeTable" runat="server">
            <tr>
                <th>Destination</th>
                <th>Language</th>
                <th>Time Period</th>
                <th></th>
            </tr>
            <tr>
                <td>
                    <asp:DropDownList ID="ddStart" CssClass="selectApp" runat="server" OnSelectedIndexChanged="ddStart_SelectedIndexChanged"></asp:DropDownList>
                </td>
                <td>
                    <asp:DropDownList ID="ddLang" CssClass="selectApp" runat="server" OnSelectedIndexChanged="ddLang_SelectedIndexChanged" AutoPostBack="true">
                        <asp:ListItem Text="English" Selected="True" Value="2"></asp:ListItem>
                        <asp:ListItem Text="Greek" Value="1"></asp:ListItem>
                        <asp:ListItem Text="Russian" Value="4"></asp:ListItem>
                    </asp:DropDownList>
                </td>
                <td>
                    <asp:DropDownList ID="ddTimePeriod" CssClass="selectApp" runat="server">
                        <asp:ListItem Text="1 Month" Selected="True" Value="1"></asp:ListItem>
                        <asp:ListItem Text="3 Months" Value="3"></asp:ListItem>
                        <asp:ListItem Text="6 Months" Value="6"></asp:ListItem>
                    </asp:DropDownList>
                </td>
                <td>
                    <asp:Button id="Goload" runat="server" Text="GO!" OnClick="Goload_Click" Font-Size="X-Large" ForeColor="#ff0000"/>
                </td>
            </tr>
        </table>
        

        
        <div id="entityPlaceHolder" runat="server">
        </div>        
        <br />
        <br />
        
        <a id="editEntiBtn" runat="server" style="float: left; padding: 6px; font-size: 1.5em;" href="javascript:void(0);"  visible="false">Edit</a>
        <asp:ImageButton runat="server" ID="refreshEntityBtn" Visible="false" ImageUrl="~/Content/img/refreshDD.png" BorderStyle="None" BackColor="Transparent" OnClick="refreshEntityBtn_Click" Style="width: 26px; margin: 5px;" />
        <asp:Button runat="server" id="GetNextEntity" Text=" OK " Visible="false" BackColor="Green" ForeColor="White" OnClick="GetNextEntity_Click" Style="float: right;"/>
        
        <asp:Button runat="server" id="RestartBtn" Text="Restart" Visible="false" OnClientClick="return ClickTrigger()" />
        <h1><asp:Literal runat="server" ID="AllValidated" Visible="false"></asp:Literal></h1>

        <script type="text/javascript">
            function ClickTrigger() {
                $("#<%= Goload.ClientID%>").click();
                return false;
            }

            function EditEntity()
            {
                DoOpenEntity(parseInt( <%= currentEntityID %>));
                return false;
            }

            $("#<%= editEntiBtn.ClientID%>").click(function () {
                EditEntity();
            });

        </script>
    </asp:Panel>
</asp:Content>


