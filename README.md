# Sample console app to demo OAuth2 flow with PKCE in IFS 10

This Application process the PKCE flow and get results from Aurena main projection for clients using IFS identity provider. 


>If you are looking for how to authenticate IFS with SSO or Azure AD as the authentication provider, then you are in the wrong place. Check [Microsoft Doc](https://docs.microsoft.com/en-us/azure/active-directory/develop/msal-overview) if you need more information on acquiring tokens from Microsoft identity platform.


## OAuth and IFS
With the release of IFS Aurena, client applications wishes to conenct to IFS needs to authenticate with OAuth2 and OpenID Connect.
More details on this can be found in the blog [IFS Authentication flow with OAuth and OpenID Connect](https://dsj23.me/2021/01/08/ifs-authentication-flow-with-oauth-and-openid-connect/).

## How to use
`>IfsOauthTest.exe https://<SERVER>:<PORT>/ <RESOURCE_PATH>`

Eg: `...\IFS-OAuth\IfsOauthTest\bin\Release>IfsOauthTest.exe https://ifsapp10:48080/ PartHandling.svc/PartCatalogSet(PartNo='TEST1')`
## TODO
A Lot to make it usable. Modify as you wish and contribute to the library!

## Technology and Tools Used
* .NET Framework 4.7.2
*  Microsoft Visual Studio Community 2019

Hope you find it useful!
