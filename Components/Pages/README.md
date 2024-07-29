## Contianer Inspection App

mudblazor => all ui
fluentvalidation => page validation + actions 
npgsql => direct table stuff 

debug additions:
    drop & init tables after each call of program.cs

bugs list:


todo:
    roll own ui widget for logins 
    page must be logged in to use 
    active user => mod by & mod perms 


database:
    postgresql: tables:
        valid-users: username : pass 
        containers-table: forms items -> only unique = ID -- append only after debug stage over?