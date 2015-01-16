﻿<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Configure.aspx.cs" Inherits="GGApps.Configure" %>
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


        <h3>Select application to Configure</h3>
        <span>*All previous updates must have finished!</span>
        <br />
        <asp:DropDownList ID="SelectApp" CssClass="selectApp" runat="server" OnSelectedIndexChanged="SelectApp_SelectedIndexChanged" AutoPostBack="true"></asp:DropDownList>

         <div width="960px;">
            <div style="float:left;">
                 <asp:RadioButtonList ID="RadioButtonLisEnvironment" CssClass="BuildAppList" runat="server" AutoPostBack="true" OnSelectedIndexChanged="RadioButtonLisEnvironment_SelectedIndexChanged" >
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
            </div>
         
            <div style="float:left;">
            <asp:RadioButtonList ID="RadioButtonListDevice" CssClass="BuildAppList" runat="server" AutoPostBack="true" OnSelectedIndexChanged="RadioButtonListDevice_SelectedIndexChanged">
                <asp:ListItem
                        Enabled="True"
                        Selected="true"
                        Text="Android"
                        Value="android"
                    />
                <asp:ListItem
                        Enabled="True"
                        Selected="false"
                        Text="iOS"
                        Value="ios"
                    />
            </asp:RadioButtonList>
            </div>
         </div>

        <div style="width:960px; float: left;">
        
            <h4>Versions.txt</h4>
            <asp:TextBox runat="server" TextMode="multiline" Columns="200" Rows="5"  ID="VersionsTxt" ></asp:TextBox>
            <h4>Configuration.txt</h4>
            <asp:TextBox runat="server" TextMode="multiline" Columns="200" Rows="20" Width="100%"  ID="ConfigurationTxt" ></asp:TextBox>

            <asp:Button runat="server" ID="SaveAll" Text="Save All Changes" OnClick="SaveAll_Click"/>
        </div>

    </asp:Panel>
</asp:Content>