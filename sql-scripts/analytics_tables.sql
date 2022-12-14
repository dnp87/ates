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

CREATE TABLE TASKS_ANALYTICS(
  ID INTEGER GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
  -- questionable, we don't want tasks to be modified in any other service
  PUBLIC_ID VARCHAR2(40) NOT NULL UNIQUE,
  NAME VARCHAR2(255) NOT NULL,
  JIRA_ID VARCHAR2(255),
  DESCRIPTION CLOB,  
  PARROT_ID INTEGER NOT NULL REFERENCES PARROTS(ID),  
  DATE_COMPLETED TIMESTAMP,
  COMPLETED_AMOUNT INTEGER NOT NULL  -- in a real system this would be a decimal
);

CREATE TABLE PROFIT_BY_PARROT_ANALYTICS
(
  ID INTEGER GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,  
  PUBLIC_ID VARCHAR2(40) NOT NULL UNIQUE,
  PARROT_ID INTEGER NOT NULL REFERENCES PARROTS(ID),
  PROFIT_AMOUNT INTEGER DEFAULT(0) NOT NULL,  -- in a real system this would be a decimal
  PROFIT_DATE DATE NOT NULL
)