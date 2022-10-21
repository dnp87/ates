CREATE USER task_tracker IDENTIFIED BY qwerty;
GRANT CONNECT, CREATE SESSION, SELECT ANY TABLE, CREATE ANY TABLE, CREATE ANY SEQUENCE, INSERT ANY TABLE, UPDATE ANY TABLE, DELETE ANY TABLE, UNLIMITED TABLESPACE TO task_tracker;

CREATE USER auth IDENTIFIED BY qwerty;
GRANT CONNECT, CREATE SESSION, SELECT ANY TABLE, CREATE ANY TABLE, CREATE ANY SEQUENCE, INSERT ANY TABLE, UPDATE ANY TABLE, DELETE ANY TABLE, UNLIMITED TABLESPACE TO auth;

CREATE USER accounting IDENTIFIED BY qwerty;
GRANT CONNECT, CREATE SESSION, SELECT ANY TABLE, CREATE ANY TABLE, CREATE ANY SEQUENCE, INSERT ANY TABLE, UPDATE ANY TABLE, DELETE ANY TABLE, UNLIMITED TABLESPACE TO accounting;

CREATE USER analytics IDENTIFIED BY qwerty;
GRANT CONNECT, CREATE SESSION, SELECT ANY TABLE, CREATE ANY TABLE, CREATE ANY SEQUENCE, INSERT ANY TABLE, UPDATE ANY TABLE, DELETE ANY TABLE, UNLIMITED TABLESPACE TO analytics;