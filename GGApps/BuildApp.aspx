<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="BuildApp.aspx.cs" Inherits="GGApps.BuildApp" %>

<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Panel runat="server" ID="mainSubPanel">
    <h2><asp:Label Text="" ID="lbl1" runat="server"></asp:Label></h2>

    <asp:RadioButtonList ID="BuildAppListID" CssClass="BuildAppList" runat="server" >
        <asp:ListItem 
            Enabled="false" Text="Export Database only" Value="DBOnly" 
            />
        <asp:ListItem 
            Enabled="false" Text="Genearate and Copy images only" Value="ImagesOnly"
            />
        <asp:ListItem
                Enabled="True"
                Selected="true"
                Text="Execute All"
                Value="FullBatch"
            />
    </asp:RadioButtonList>

    <asp:Button runat="server" Text="Go!" OnClick="OnCheckBox_Click" />
    </asp:Panel>    

    <div runat="server" id="ExecutionMessages">

    </div>
</asp:Content>
