CREATE TABLE ROLES (
  ID INTEGER NOT NULL PRIMARY KEY,
  NAME VARCHAR2(255) NOT NULL UNIQUE
);

CREATE TABLE PARROTS(
  ID INTEGER GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,  
  PUBLIC_ID VARCHAR2(40) NOT NULL UNIQUE,
  EMAIL VARCHAR2(2000) NOT NULL UNIQUE,
  NAME VARCHAR2(255) NOT NULL UNIQUE,
  ROLE_ID INTEGER NOT NULL REFERENCES ROLES(ID)
);

CREATE TABLE TASKS(
  ID INTEGER GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
  -- questionable, we don't want parrots to be modified in any other service
  PUBLIC_ID VARCHAR2(40) NOT NULL UNIQUE,
  NAME VARCHAR2(255) NOT NULL,
  DESCRIPTION CLOB,
  -- STATUS: 1 - active, 2 - completed; TODO: create separate status table
  STATUS_ID INTEGER DEFAULT 1 NOT NULL,
  PARROT_ID INTEGER NOT NULL REFERENCES PARROTS(ID),
  CONSTRAINT STATUS_VALUE_CHECK CHECK(STATUS_ID IN (1, 2)) 
);