# [Testing SSL in ASP.NET Core on a Mac](https://peteskelly.com/testing-ssl-in-asp-net-core-mac/)

Back in October 2016, Shawn Wildermuth published [Testing SSL in ASP.NET Core](https://wildermuth.com/2016/10/26/Testing-SSL-in-ASP-NET-Core). I came across this post while trying to get HTTPS working with ASP.NET Core on a Mac. Sadly, this post was specific to Windows.

## Create an ASP.NET Core MVC Project

To start, create a new folder and then create a new MVC project. Open a terminal, or use Visual Studio Codes's terminal window, and type the following.

```bash
mkdir CoreWebAppHttps && cd ...
dotnet new mvc
dotnet restore
dotnet build
dotnet run
```

Open a browser and navigate to [http://localhost:5000](http://localhost:5000/). You should see the familiar MVC application pages.

![http_mvc](https://peteskelly.com/content/images/2017/03/http_mvc.png)

## Enabling SSL

Shawn showed how to use SSL within Visual Studio and IISExpress and without IISExpress on Windows. To enable SSL for ASP.NET Core on a Mac, use OpenSSL. If you have .NET Core on a Mac, you should already have OpenSSL installed, but if you don't, install it now using the [.NET Core instructions to install OpenSSL](https://www.microsoft.com/net/core#macos).

Once you have OpenSSL installed, run the following from a terminal in the root folder of the web application we just created.

```bash
openssl genrsa -out key.pem 2048
openssl req -new -sha256 -key key.pem -out csr.csr
```

The second command will prompt you for some information as indicated below. Be sure to answer with `localhost` when prompted for the FQDN or Name.

![img](https://peteskelly.com/content/images/2017/03/cert-pem.png)

Next, run the following to create a certificate.

```bash
openssl req -x509 -sha256 -days 365 -key key.pem -in csr.csr -out certificate.pem
```

Now create a .pfk file using the following:

```bash
openssl pkcs12 -export -out localhost.pfx -inkey key.pem -in certificate.pem
```

Note the password you used for the .pfx file, you will need this later. After you finished these commands, you should have a localhost.pfx file at the root of your project, like the following:

![img](https://peteskelly.com/content/images/2017/03/Screen-Shot-2017-03-30-at-9.31.32-PM.png)

### Add the Kestrel Https Extensions Package

Now execute the following from the terminal to add the Https extension methods Kestrel needs to serve https pages.

```bash
dotnet add package Microsoft.AspNetCore.Server.Kestrel.Https
```

You can also edit the CoreWebHttps.csproj file if you want (you just need one). Add the following in the `ItemGroup` with the other package references.

```xml
<PackageReference Include="Microsoft.AspNetCore.Server.Kestrel.Https" Version="1.1.1" />
```

Now run the following to restore the packages. if you are already in Visual Studio Code, it may prompt you to restore the packages as well.

```bash
dotnet restore 
```

### Update Program.cs

Almost there. Open Program.cs and then edit the file to be the following. Note that lines 15 - 18 are the relevant lines that need to change. If you get a warning about `options.UseHttps()` don't forget to run `dotnet restore`. Also, don't forget to use the password you noted earlier.

![img](https://peteskelly.com/content/images/2017/03/Screen-Shot-2017-03-30-at-9.53.25-PM.png)

### Run your SSL site

Now simply execute `dotnet run` and load your page in a browser at `https://localhost:8080`. You should now see your pages in SSL.

![img](https://peteskelly.com/content/images/2017/03/https_mvc.png)

If you want to skip the unsafe site warnings, you can add the certificate to your Keychain (System) and this will give you the familiar green lock for the url.

![img](https://peteskelly.com/content/images/2017/03/Screen-Shot-2017-03-30-at-10.00.33-PM.png)

## SSL principle

Secure Sockets Layer

- Used to protect communication between client and server
- Based on Concepts of Trust + Encryption

![Symmetric Encryption](http://om1o84p1p.bkt.clouddn.com/1501398241.png)

![ASymmetric Encryption](http://om1o84p1p.bkt.clouddn.com/1501398347.png)

![SSL Handshake](http://om1o84p1p.bkt.clouddn.com/1501399951.png?imageMogr2/thumbnail/!70p)

SSL uses both Symmetric and ASymmetric Encryption. The reason why not just using ASymmetric is that ASymmetric takes longer time.