<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Configure.aspx.cs" Inherits="GGApps.Configure" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="FeaturedContent" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">

      <div class="sub">
        <ul id="menu">
            <li class="selected"><a href="Configure.aspx">Configure</a></li>
            <li><a href="HomePage.aspx">Home Page</a></li>
            <li><a href="Promo.aspx">Promo</a></li>
            <li><a href="Publish.aspx">Publish</a></li>
        </ul>
    </div>
     <asp:Panel ID="LoginViewImportant" CssClass="LoginViewImportant" runat="server">

        <asp:RadioButtonList ID="EnvironmentConfig" CssClass="BuildAppList" runat="server" OnSelectedIndexChanged="EnvironmentConfig_SelectedIndexChanged" >
            <asp:ListItem
                    Enabled="True"
                    Selected="true"
                    Text="Staging"
                    Value="Staging"
                />
            <asp:ListItem
                    Enabled="True"
                    Selected="false"
                    Text="Production"
                    Value="Production"
                />
        </asp:RadioButtonList>


         <h3>Select application to Configure</h3>
         <span>*All previous updates must have finished!</span>
            
        <br />
        <asp:DropDownList ID="SelectApp" Width="200" runat="server"></asp:DropDownList>

        <div style="width:100%">
        
            <asp:Literal ID="Literal1" runat="server">Versions.txt</asp:Literal>
            <asp:TextBox runat="server" TextMode="multiline" Columns="50" Rows="5"  ID="VersionsTxt" ></asp:TextBox>
            <asp:Literal ID="Literal2" runat="server">Configuration.txt</asp:Literal>
            <asp:TextBox runat="server" TextMode="multiline" Columns="50" Rows="5"  ID="ConfigurationTxt" ></asp:TextBox>


        </div>

    </asp:Panel>
</asp:Content>
