# Sample cmdline app to demo OAuth2 flow with PKCE with IFS 10

This Application process the PKCE flow and get results from Aurena main projection

## OAuth and IFS
With the release of IFS Aurena, client applications wishes to conenct to IFS needs to authenticate with OAuth2 and OpenID Connect.
More details on this can be found in the blog [IFS Authentication flow with OAuth and OpenID Connect](https://dsj23.me/2021/01/08/ifs-authentication-flow-with-oauth-and-openid-connect/).

## How to use
Change `ifsconn.Ifs_url` in /IfsOauthTest/Program.cs
Give any valid search criteria fro Projection call `PartHandling.svc/PartCatalogSet(PartNo='TEST1')`

## TODO
A Lot to make it usable. Modify as you wish!

## Technology and Tools Used
* .NET Framework 4.7.2
*  Microsoft Visual Studio Community 2019
