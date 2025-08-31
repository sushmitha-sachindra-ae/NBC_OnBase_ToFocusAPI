steps to deploy to IIS:

1. install .net 8.0.0 hosting bundle
2.copy the publish folder to c://inetpub/wwwroot
3.create app pool 
 name : core
.net clr: No managed code
managed pipeline : Integrated
identity: Localsysytem

4.convert to application for the published folder
  select pool: core(which is created in the previous step)
5.copy XMLNS files folder  inside the publish folder
