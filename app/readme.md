VARIABLES DE AMBIENTE DE LA STATIC WEB APP

az staticwebapp appsettings set --name electa-codes-test01 --resource-group electa-codes-test01_group --setting-names "MailService__MailFrom=info@codes.com.ar"
az staticwebapp appsettings set --name electa-codes-test01 --resource-group electa-codes-test01_group --setting-names "MailService__MailFromTitulo=Electa Trading"
az staticwebapp appsettings set --name electa-codes-test01 --resource-group electa-codes-test01_group --setting-names "MailService__SmtpClient=smtp-relay.gmail.com"
az staticwebapp appsettings set --name electa-codes-test01 --resource-group electa-codes-test01_group --setting-names "MailService__SmtpClientPort=587"
az staticwebapp appsettings set --name electa-codes-test01 --resource-group electa-codes-test01_group --setting-names "MailService__SmtpClientEnableSSL=true"
az staticwebapp appsettings set --name electa-codes-test01 --resource-group electa-codes-test01_group --setting-names "MailService__SmtpClientUseDefaultCredentials=false"
az staticwebapp appsettings set --name electa-codes-test01 --resource-group electa-codes-test01_group --setting-names "MailService__IsBodyHtml=true"
az staticwebapp appsettings set --name electa-codes-test01 --resource-group electa-codes-test01_group --setting-names "MailService__ProcessToken=******"
az staticwebapp appsettings set --name electa-codes-test01 --resource-group electa-codes-test01_group --setting-names "MailService__MailTo=dcastro@codes.com.ar"
az staticwebapp appsettings set --name electa-codes-test01 --resource-group electa-codes-test01_group --setting-names "MailService__Subject=Contacto por Electa Trading"
az staticwebapp appsettings set --name electa-codes-test01 --resource-group electa-codes-test01_group --setting-names "Landing__UrlVerify=https://www.google.com/recaptcha/api/siteverify"
az staticwebapp appsettings set --name electa-codes-test01 --resource-group electa-codes-test01_group --setting-names "Landing__GoogleToken=*****