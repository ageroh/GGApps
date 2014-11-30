<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="GGApps._Default" %>

<asp:Content runat="server" ID="FeaturedContent" ContentPlaceHolderID="FeaturedContent">


        <asp:LoginView ID="LoginView1" runat="server" >
        <AnonymousTemplate>
            <section class="featured">
                <div class="content-wrapper">
                    <hgroup class="title">
                        <h1><%: Title %>.</h1>
                        <h2>Please connect in order to move with app update for GreekGuide.</h2>
                    </hgroup>
                    <p>
                                       
                    </p>
                </div>
            </section>
        </AnonymousTemplate>
        <LoggedInTemplate>
            <section class="featured">
                <div class="content-wrapper">
                    <hgroup class="title">
                        <h2>Select the App to update for GreekGuide</h2>
                    </hgroup>
                </div>
            </section>
        </LoggedInTemplate>
    </asp:LoginView>
</asp:Content>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">

    <asp:LoginView ID="LoginViewImportant" runat="server" >
        <AnonymousTemplate>
            <h3>Please first login!</h3>
        </AnonymousTemplate>
        <LoggedInTemplate>
            
            <h3>Select application to update (one at a time)</h3>
            <ol class="round">
                <li class="one">
                    <asp:DropDownList ID="ddStart" BackColor="GreenYellow" runat="server"></asp:DropDownList>
                    <asp:Button id="GoFirst" runat="server" Text="Build Report" OnClick="GoFirst_Click"/>
                </li>
                <li class="two">
                   Continue...
                </li>
            </ol>
            
         
            <textarea readonly id="MainTextArea" rows="400" cols="80"  style="height:200px;">
                <asp:Literal runat="server" id="txtEditor1" />
            </textarea>

            <div runat="server" id="reportDiv">
            </div>
         
        </LoggedInTemplate>
    </asp:LoginView>
</asp:Content>





                        
                   