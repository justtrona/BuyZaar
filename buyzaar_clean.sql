-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Host: 127.0.0.1
-- Generation Time: May 17, 2026 at 07:50 PM
-- Server version: 10.4.32-MariaDB
-- PHP Version: 8.2.12

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Database: buyzaar_db
--

-- --------------------------------------------------------

--
-- Table structure for table aspnetroleclaims
--

CREATE TABLE aspnetroleclaims (
  Id int(11) NOT NULL,
  RoleId varchar(255) NOT NULL,
  ClaimType longtext DEFAULT NULL,
  ClaimValue longtext DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table aspnetroles
--

CREATE TABLE aspnetroles (
  Id varchar(255) NOT NULL,
  Name varchar(256) DEFAULT NULL,
  NormalizedName varchar(256) DEFAULT NULL,
  ConcurrencyStamp longtext DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table aspnetroles
--

INSERT INTO aspnetroles (Id, Name, NormalizedName, ConcurrencyStamp) VALUES
('38c43561-589a-449a-82fa-156290f771ec', 'Admin', 'ADMIN', '3838398e-323b-4f51-ad9e-d850af78fee4'),
('428234e5-ce39-4518-b5f5-98731e67b08d', 'Seller', 'SELLER', 'a5490dca-739e-4b27-889d-463abd43212f'),
('7fa38e5a-f4c0-4dc6-bd4c-c296ca5640f2', 'Rider', 'RIDER', 'ced3b23d-0f8c-4c07-bd3e-fb22a65d6bea'),
('7fd92bb2-0f07-4e87-835a-168d408ab590', 'SuperAdmin', 'SUPERADMIN', '1fdc69a0-aced-4396-9940-d3dac799b9df'),
('add351f5-fbcf-4b83-8877-165de2761d56', 'Shopper', 'SHOPPER', '747e7354-d1f6-41a0-815b-be488946b8be');

-- --------------------------------------------------------

--
-- Table structure for table aspnetuserclaims
--

CREATE TABLE aspnetuserclaims (
  Id int(11) NOT NULL,
  UserId varchar(255) NOT NULL,
  ClaimType longtext DEFAULT NULL,
  ClaimValue longtext DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table aspnetuserlogins
--

CREATE TABLE aspnetuserlogins (
  LoginProvider varchar(128) NOT NULL,
  ProviderKey varchar(128) NOT NULL,
  ProviderDisplayName longtext DEFAULT NULL,
  UserId varchar(255) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table aspnetuserroles
--

CREATE TABLE aspnetuserroles (
  UserId varchar(255) NOT NULL,
  RoleId varchar(255) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table aspnetuserroles
--

INSERT INTO aspnetuserroles (UserId, RoleId) VALUES
('0623d7d6-1ee4-484d-b647-b53df173fb38', '428234e5-ce39-4518-b5f5-98731e67b08d'),
('0623d7d6-1ee4-484d-b647-b53df173fb38', 'add351f5-fbcf-4b83-8877-165de2761d56'),
('07cd35c6-fce8-4823-9a76-00c69ad7ba19', '7fd92bb2-0f07-4e87-835a-168d408ab590'),
('15fec38d-d2dd-442c-b232-844e745235e4', '7fa38e5a-f4c0-4dc6-bd4c-c296ca5640f2'),
('26385ff3-cc02-4511-b0fc-6fd8beb0f393', 'add351f5-fbcf-4b83-8877-165de2761d56'),
('31a998f2-b606-4aca-8f70-0adc3e8b53bc', 'add351f5-fbcf-4b83-8877-165de2761d56'),
('8048a2ad-9b70-4ddc-ae4a-49004f717b24', '7fa38e5a-f4c0-4dc6-bd4c-c296ca5640f2'),
('8869959b-2a71-4a73-a3c1-1f803c5f444c', '38c43561-589a-449a-82fa-156290f771ec'),
('988a899f-5bab-4e80-a27d-d82206c2be3a', 'add351f5-fbcf-4b83-8877-165de2761d56'),
('e20f4d92-3908-4527-a13c-f3a6ccb769a2', '428234e5-ce39-4518-b5f5-98731e67b08d'),
('e20f4d92-3908-4527-a13c-f3a6ccb769a2', 'add351f5-fbcf-4b83-8877-165de2761d56'),
('e2f8c79e-49c8-42eb-8a2f-99db385d2b06', '7fa38e5a-f4c0-4dc6-bd4c-c296ca5640f2'),
('e510af4e-e904-451b-a260-a3eff3ab9387', '38c43561-589a-449a-82fa-156290f771ec');

-- --------------------------------------------------------

--
-- Table structure for table aspnetusers
--

CREATE TABLE aspnetusers (
  Id varchar(255) NOT NULL,
  FullName longtext NOT NULL,
  UserName varchar(256) DEFAULT NULL,
  NormalizedUserName varchar(256) DEFAULT NULL,
  Email varchar(256) DEFAULT NULL,
  NormalizedEmail varchar(256) DEFAULT NULL,
  EmailConfirmed tinyint(1) NOT NULL,
  PasswordHash longtext DEFAULT NULL,
  SecurityStamp longtext DEFAULT NULL,
  ConcurrencyStamp longtext DEFAULT NULL,
  PhoneNumber longtext DEFAULT NULL,
  PhoneNumberConfirmed tinyint(1) NOT NULL,
  TwoFactorEnabled tinyint(1) NOT NULL,
  LockoutEnd datetime(6) DEFAULT NULL,
  LockoutEnabled tinyint(1) NOT NULL,
  AccessFailedCount int(11) NOT NULL,
  IsVerified tinyint(1) NOT NULL DEFAULT 0,
  VerifiedAt datetime(6) DEFAULT NULL,
  ShopName longtext DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table aspnetusers
--

INSERT INTO aspnetusers (Id, FullName, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnd, LockoutEnabled, AccessFailedCount, IsVerified, VerifiedAt, ShopName) VALUES
('0623d7d6-1ee4-484d-b647-b53df173fb38', 'Jieun', 'iujieun', 'IUJIEUN', 'iujieungaming@gmail.com', 'IUJIEUNGAMING@GMAIL.COM', 1, 'AQAAAAIAAYagAAAAEA3o78yUvbPYtaac9V44/tLTW7b7HrurDn6HITGASFtSLuFK94fEXTA44okXD0XFWQ==', 'OMSYTXBO4J7SFHVVDPEVA6MU6VGTAMD7', '9c795baa-e014-4de2-bec3-217f56d7df1c', NULL, 0, 0, NULL, 1, 0, 1, '2026-04-18 15:44:48.750180', NULL),
('07cd35c6-fce8-4823-9a76-00c69ad7ba19', 'Super Admin', 'superadmin', 'SUPERADMIN', 'superadmin@buyzaar.com', 'SUPERADMIN@BUYZAAR.COM', 0, 'AQAAAAIAAYagAAAAEOLz7ouivN1qNY9V0MqQpOrXvRsTs0rOPxXGcIdFuliO8QLHOWfT9OzX2hNkdc4IKg==', '2YTYM3I453BCM7MR4V5PYZHDGII3H2CV', '26674e61-69a1-4930-9122-13bddbbe9a18', NULL, 0, 0, NULL, 1, 0, 0, NULL, NULL),
('15fec38d-d2dd-442c-b232-844e745235e4', 'Neo Kenneth', 'neoken062300@gmail.com', 'NEOKEN062300@GMAIL.COM', 'neoken062300@gmail.com', 'NEOKEN062300@GMAIL.COM', 1, 'AQAAAAIAAYagAAAAEFqy9LpFva8NqDpB+m5fkYPEciwiGRv1PMoaSzEOjCtr+9UOUsaYcrT3298WTZ/UoA==', 'EPRYRKSJMLONKATRO6Y4YMWKDFJMUFWW', '064005cd-0e6d-4745-93a4-379acaa07818', '09770795986', 0, 0, NULL, 1, 0, 1, '2026-05-08 15:24:19.676675', NULL),
('26385ff3-cc02-4511-b0fc-6fd8beb0f393', 'testshopper', 'testshopper', 'TESTSHOPPER', 'testshopper1@buyzaar.com', 'TESTSHOPPER1@BUYZAAR.COM', 0, 'AQAAAAIAAYagAAAAEHSR13j6XW7CUi4/Xtfq4lqrubxXxAL/uB9RFvX0PQstLsmsQDjq+MEsklct8kF6ng==', 'HTOFXVVTQF27B5UQ3W4HKMUXE57HXQKH', '9e2fb689-8e6f-49b8-8fef-cf588b7aaf9d', NULL, 0, 0, NULL, 1, 0, 0, NULL, NULL),
('31a998f2-b606-4aca-8f70-0adc3e8b53bc', 'Ezekiel Pol', 'Zeke', 'ZEKE', 'samyangcarbonara122500@gmail.com', 'SAMYANGCARBONARA122500@GMAIL.COM', 1, 'AQAAAAIAAYagAAAAEGXjBtaVJ33J1pEkRaKK14wTz0AzvmwI+TzVQvMi0SUxdvds7hATPdVMYyZiaieKUw==', 'UYQ3GYHVGOS2ZO2KESX3AWMLE7WFTLFH', '7e3ba9f0-bd98-4261-9146-fc2b8cc42de3', NULL, 0, 0, NULL, 1, 0, 1, '2026-05-02 10:52:31.157354', NULL),
('8048a2ad-9b70-4ddc-ae4a-49004f717b24', 'Luwi Paloso', 'kirbykiero@gmail.com', 'KIRBYKIERO@GMAIL.COM', 'kirbykiero@gmail.com', 'KIRBYKIERO@GMAIL.COM', 1, 'AQAAAAIAAYagAAAAEJlKj2M+Mz7R93OJbjc0/OjSi4b+50Hdg1VOJ7YerFb5tOrniJT2m7f/lGc/c05ghg==', 'NP7C5DKMWBXYYBGU2FIPK5UBAW52KG5C', 'cacd40fa-1251-419b-8311-06d1cd7b4693', '09123467777', 0, 0, '9999-12-31 23:59:59.999999', 1, 0, 0, NULL, NULL),
('8869959b-2a71-4a73-a3c1-1f803c5f444c', 'testadmin102', 'testadmin102', 'TESTADMIN102', 'testadmin102@buyzaar.com', 'TESTADMIN102@BUYZAAR.COM', 0, 'AQAAAAIAAYagAAAAEGPpfN+vM2Z9VWFRXw6IZvrLvXEw7pcIHsNgpUTVkGMLpEIIN8U5korjGf7KIEAGNQ==', '2EROOJDDOHGH4MGDC2OWDSFLD5ONUHW2', '0dd81be9-a0f6-454e-a4eb-57e07dcd53ba', NULL, 0, 0, NULL, 1, 0, 0, NULL, NULL),
('988a899f-5bab-4e80-a27d-d82206c2be3a', 'Lava Hings', 'lavahings', 'LAVAHINGS', 'mrlavalava048@gmail.com', 'MRLAVALAVA048@GMAIL.COM', 1, 'AQAAAAIAAYagAAAAEGfwDyEsbY65e52dBIQWiR3CBHFMt1+1LDHFVdppbmHSh33T+Wk6f1bUwwCTRcnL6A==', 'S6BK3T6OGEZRSCROLGEFCNAM5GABKX4Y', '557a72f7-3f64-4438-a193-76fa317ff94c', NULL, 0, 0, NULL, 1, 0, 1, '2026-05-08 05:57:13.578835', NULL),
('e20f4d92-3908-4527-a13c-f3a6ccb769a2', 'test 101', 'test101', 'TEST101', 'test101@buyzaar.com', 'TEST101@BUYZAAR.COM', 0, 'AQAAAAIAAYagAAAAEDbpb2n77e5KxZ8u5PNcpMcIeQiNZLXV2GoWkITa2UWQ/XMZOWUUz5ktopJzAXKlJg==', '3JPS5N7GHDD2ERK6KRFG7EUXI3RVYNHM', '2e9b90b1-e6ff-4009-b046-a4a0c940cbb9', NULL, 0, 0, NULL, 1, 0, 0, NULL, NULL),
('e2f8c79e-49c8-42eb-8a2f-99db385d2b06', 'Ren Niel ', 'formycapstone@gmail.com', 'FORMYCAPSTONE@GMAIL.COM', 'formycapstone@gmail.com', 'FORMYCAPSTONE@GMAIL.COM', 1, 'AQAAAAIAAYagAAAAEL4xH7dEzXlpjCf8ADlhqkeifN5YbCjaf6yyBBtdcSx/6+Xs6pcqyhPFi4MNsfP0gg==', 'HYWUDRCXMJ4MINSQLB4ER33D4WAMVWRG', '63f9f38f-c2a6-4353-9266-86053cd360fd', '09123456780', 0, 0, NULL, 1, 0, 1, '2026-05-08 11:46:44.667861', NULL),
('e510af4e-e904-451b-a260-a3eff3ab9387', 'Sync Kit', 'synckit', 'SYNCKIT', 'synckitvx@gmail.com', 'SYNCKITVX@GMAIL.COM', 1, 'AQAAAAIAAYagAAAAEMqsti59o48xHpdimGb8u9cxSUUKy3vwCT8BGwIcmtXkuUcmyT7eN9/rXchz+q8ygA==', 'WGB7JCSFRVJAPTWJGIN3RRBW7DZHXAOL', 'cfe50b8a-5099-4efa-bbf4-7b9ee81ddc28', '09123467777', 0, 0, NULL, 1, 0, 1, '2026-05-14 05:29:04.094265', NULL);

-- --------------------------------------------------------

--
-- Table structure for table aspnetusertokens
--

CREATE TABLE aspnetusertokens (
  UserId varchar(255) NOT NULL,
  LoginProvider varchar(128) NOT NULL,
  Name varchar(128) NOT NULL,
  Value longtext DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table auditlogs
--

CREATE TABLE auditlogs (
  Id int(11) NOT NULL,
  AdminId varchar(255) NOT NULL,
  Action varchar(100) NOT NULL,
  EntityType varchar(100) NOT NULL,
  EntityId varchar(100) NOT NULL,
  Description varchar(1000) DEFAULT NULL,
  CreatedAt datetime(6) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table auditlogs
--

INSERT INTO auditlogs (Id, AdminId, Action, EntityType, EntityId, Description, CreatedAt) VALUES
(1, '8869959b-2a71-4a73-a3c1-1f803c5f444c', 'Assign Rider', 'Order', '8', 'Admin assigned Order #8 to rider: Neo Kenneth', '2026-05-11 01:27:43.598468'),
(2, '8869959b-2a71-4a73-a3c1-1f803c5f444c', 'Assign Rider', 'Order', '6', 'Admin assigned Order #6 to rider: Neo Kenneth', '2026-05-13 15:14:53.211113'),
(3, '8869959b-2a71-4a73-a3c1-1f803c5f444c', 'Deactivate User', 'User', '8048a2ad-9b70-4ddc-ae4a-49004f717b24', 'Admin deactivated user account: kirbykiero@gmail.com', '2026-05-13 17:00:27.065767'),
(4, '07cd35c6-fce8-4823-9a76-00c69ad7ba19', 'Update Settings', 'System Settings', '1', 'Updated registration control settings.', '2026-05-14 23:45:25.881686'),
(5, 'e510af4e-e904-451b-a260-a3eff3ab9387', 'Assign Rider', 'Order', '9', 'Admin assigned Order #9 to rider: Neo Kenneth', '2026-05-15 02:21:38.876084'),
(6, '07cd35c6-fce8-4823-9a76-00c69ad7ba19', 'Activate User', 'User', '8048a2ad-9b70-4ddc-ae4a-49004f717b24', 'Activated user account Luwi Paloso (kirbykiero@gmail.com).', '2026-05-15 03:56:55.751173'),
(7, '07cd35c6-fce8-4823-9a76-00c69ad7ba19', 'Deactivate User', 'User', '8048a2ad-9b70-4ddc-ae4a-49004f717b24', 'Deactivated user account Luwi Paloso (kirbykiero@gmail.com).', '2026-05-15 04:02:52.703285'),
(8, '07cd35c6-fce8-4823-9a76-00c69ad7ba19', 'Test Audit', 'System', 'TEST', 'This is a test audit log from SuperAdmin.', '2026-05-15 04:13:52.328963'),
(9, '07cd35c6-fce8-4823-9a76-00c69ad7ba19', 'Test Audit', 'System', 'TEST', 'This is a test audit log from SuperAdmin.', '2026-05-15 04:13:53.877132'),
(10, '07cd35c6-fce8-4823-9a76-00c69ad7ba19', 'Test Audit', 'System', 'TEST', 'This is a test audit log from SuperAdmin.', '2026-05-15 04:13:54.466475'),
(11, '07cd35c6-fce8-4823-9a76-00c69ad7ba19', 'Test Audit', 'System', 'TEST', 'This is a test audit log from SuperAdmin.', '2026-05-15 04:13:54.653842'),
(12, '07cd35c6-fce8-4823-9a76-00c69ad7ba19', 'Test Audit', 'System', 'TEST', 'This is a test audit log from SuperAdmin.', '2026-05-15 04:13:56.777639'),
(13, '07cd35c6-fce8-4823-9a76-00c69ad7ba19', 'Test Audit', 'System', 'TEST', 'This is a test audit log from SuperAdmin.', '2026-05-15 04:13:56.973246'),
(14, '07cd35c6-fce8-4823-9a76-00c69ad7ba19', 'Test Audit', 'System', 'TEST', 'This is a test audit log from SuperAdmin.', '2026-05-15 04:13:57.141838'),
(15, '07cd35c6-fce8-4823-9a76-00c69ad7ba19', 'Test Audit Log', 'System', 'TEST', 'This is a test audit log created from the SuperAdmin Audit Logs page.', '2026-05-15 04:14:36.995445'),
(16, '07cd35c6-fce8-4823-9a76-00c69ad7ba19', 'Test Audit Log', 'System', 'TEST', 'This is a test audit log created from the SuperAdmin Audit Logs page.', '2026-05-15 04:14:37.881433'),
(17, '07cd35c6-fce8-4823-9a76-00c69ad7ba19', 'Test Audit Log', 'System', 'TEST', 'This is a test audit log created from the SuperAdmin Audit Logs page.', '2026-05-15 04:14:38.492782'),
(18, '07cd35c6-fce8-4823-9a76-00c69ad7ba19', 'Activate User', 'User', '8048a2ad-9b70-4ddc-ae4a-49004f717b24', 'Activated user account Luwi Paloso (kirbykiero@gmail.com).', '2026-05-15 04:15:18.652531'),
(19, '07cd35c6-fce8-4823-9a76-00c69ad7ba19', 'Test Audit Log', 'System', 'TEST', 'This is a test audit log created from the SuperAdmin Audit Logs page.', '2026-05-15 04:15:29.247029'),
(20, '07cd35c6-fce8-4823-9a76-00c69ad7ba19', 'Deactivate User', 'User', '8048a2ad-9b70-4ddc-ae4a-49004f717b24', 'Deactivated user account Luwi Paloso (kirbykiero@gmail.com).', '2026-05-15 04:15:45.168869'),
(21, '07cd35c6-fce8-4823-9a76-00c69ad7ba19', 'Activate User', 'User', '8048a2ad-9b70-4ddc-ae4a-49004f717b24', 'Activated user account Luwi Paloso (kirbykiero@gmail.com).', '2026-05-15 04:16:42.438801'),
(22, '07cd35c6-fce8-4823-9a76-00c69ad7ba19', 'Deactivate User', 'User', '8048a2ad-9b70-4ddc-ae4a-49004f717b24', 'Deactivated user account Luwi Paloso (kirbykiero@gmail.com).', '2026-05-15 04:17:04.141761'),
(23, '07cd35c6-fce8-4823-9a76-00c69ad7ba19', 'Activate User', 'User', '8048a2ad-9b70-4ddc-ae4a-49004f717b24', 'Activated user account Luwi Paloso (kirbykiero@gmail.com).', '2026-05-15 04:18:19.259674'),
(24, '07cd35c6-fce8-4823-9a76-00c69ad7ba19', 'Test Audit Log', 'System', 'TEST', 'This is a test audit log created from the SuperAdmin Audit Logs page.', '2026-05-15 04:19:57.672716'),
(25, '07cd35c6-fce8-4823-9a76-00c69ad7ba19', 'Deactivate User', 'User', '8048a2ad-9b70-4ddc-ae4a-49004f717b24', 'Deactivated user account Luwi Paloso (kirbykiero@gmail.com).', '2026-05-15 04:20:05.634419'),
(26, 'e510af4e-e904-451b-a260-a3eff3ab9387', 'Assign Rider', 'Order', '15', 'Admin assigned Order #15 to rider: Neo Kenneth', '2026-05-15 15:59:30.702140'),
(27, 'e510af4e-e904-451b-a260-a3eff3ab9387', 'Assign Rider', 'Order', '18', 'Admin assigned Order #18 to rider: Neo Kenneth', '2026-05-15 16:04:35.803233');

-- --------------------------------------------------------

--
-- Table structure for table cartitems
--

CREATE TABLE cartitems (
  Id int(11) NOT NULL,
  ShopperId varchar(255) NOT NULL,
  ProductId int(11) NOT NULL,
  Quantity int(11) NOT NULL,
  SelectedVariant varchar(500) DEFAULT NULL,
  SelectedSize varchar(500) DEFAULT NULL,
  CreatedAt datetime(6) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table cartitems
--

INSERT INTO cartitems (Id, ShopperId, ProductId, Quantity, SelectedVariant, SelectedSize, CreatedAt) VALUES
(1, '0623d7d6-1ee4-484d-b647-b53df173fb38', 4, 1, 'White', 'L', '2026-05-01 03:32:01.782797'),
(3, '31a998f2-b606-4aca-8f70-0adc3e8b53bc', 4, 4, 'Beige', 'M', '2026-05-02 19:01:21.996058');

-- --------------------------------------------------------

--
-- Table structure for table categories
--

CREATE TABLE categories (
  Id int(11) NOT NULL,
  Name longtext NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table commissionrates
--

CREATE TABLE commissionrates (
  Id int(11) NOT NULL,
  RatePercentage decimal(65,30) NOT NULL,
  IsActive tinyint(1) NOT NULL,
  CreatedAt datetime(6) NOT NULL,
  UpdatedAt datetime(6) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table orderitems
--

CREATE TABLE orderitems (
  Id int(11) NOT NULL,
  OrderId int(11) NOT NULL,
  ProductId int(11) NOT NULL,
  Quantity int(11) NOT NULL,
  Price decimal(65,30) NOT NULL,
  Subtotal decimal(65,30) NOT NULL,
  SelectedVariant longtext DEFAULT NULL,
  SelectedSize longtext DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table orderitems
--

INSERT INTO orderitems (Id, OrderId, ProductId, Quantity, Price, Subtotal, SelectedVariant, SelectedSize) VALUES
(5, 5, 3, 1, 2250.000000000000000000000000000000, 2250.000000000000000000000000000000, 'Army Green', NULL),
(6, 6, 4, 1, 6750.000000000000000000000000000000, 6750.000000000000000000000000000000, 'Beige', 'M'),
(7, 7, 1, 1, 500.000000000000000000000000000000, 500.000000000000000000000000000000, NULL, NULL),
(9, 9, 1, 1, 500.000000000000000000000000000000, 500.000000000000000000000000000000, NULL, NULL),
(10, 10, 3, 1, 2250.000000000000000000000000000000, 2250.000000000000000000000000000000, 'Army Green', NULL),
(15, 15, 3, 1, 2250.000000000000000000000000000000, 2250.000000000000000000000000000000, 'Army Green', NULL),
(17, 17, 4, 1, 6750.000000000000000000000000000000, 6750.000000000000000000000000000000, 'White', 'XXL'),
(18, 18, 4, 1, 6750.000000000000000000000000000000, 6750.000000000000000000000000000000, 'White', 'XXL'),
(19, 19, 4, 1, 6750.000000000000000000000000000000, 6750.000000000000000000000000000000, 'White', 'XL');

-- --------------------------------------------------------

--
-- Table structure for table orders
--

CREATE TABLE orders (
  Id int(11) NOT NULL,
  ShopperId longtext NOT NULL,
  DeliveryAddress longtext NOT NULL,
  TotalAmount decimal(65,30) NOT NULL,
  Status longtext NOT NULL,
  CreatedAt datetime(6) NOT NULL,
  ContactNumber longtext NOT NULL,
  ReceiverName longtext NOT NULL,
  ShippingFee decimal(65,30) NOT NULL DEFAULT 0.000000000000000000000000000000,
  AcceptedAt datetime(6) DEFAULT NULL,
  AssignedAt datetime(6) DEFAULT NULL,
  DeliveredAt datetime(6) DEFAULT NULL,
  DeliveryStatus longtext NOT NULL,
  PickedUpAt datetime(6) DEFAULT NULL,
  RiderId varchar(255) DEFAULT NULL,
  FailedDeliveryAt datetime(6) DEFAULT NULL,
  FailedDeliveryReason longtext DEFAULT NULL,
  ReturnToSellerAt datetime(6) DEFAULT NULL,
  CancellationAdminNote longtext DEFAULT NULL,
  CancellationReason longtext DEFAULT NULL,
  CancellationRequestStatus longtext DEFAULT NULL,
  CancellationRequestedAt datetime(6) DEFAULT NULL,
  CancellationReviewedAt datetime(6) DEFAULT NULL,
  IsPreparing tinyint(1) NOT NULL DEFAULT 0,
  IsReadyForPickup tinyint(1) NOT NULL DEFAULT 0,
  PreparingAt datetime(6) DEFAULT NULL,
  ReadyForPickupAt datetime(6) DEFAULT NULL,
  ReturnedAt datetime(6) DEFAULT NULL,
  ReturnedByRiderId longtext DEFAULT NULL,
  ReturnedByRiderId1 int(11) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table orders
--

INSERT INTO orders (Id, ShopperId, DeliveryAddress, TotalAmount, Status, CreatedAt, ContactNumber, ReceiverName, ShippingFee, AcceptedAt, AssignedAt, DeliveredAt, DeliveryStatus, PickedUpAt, RiderId, FailedDeliveryAt, FailedDeliveryReason, ReturnToSellerAt, CancellationAdminNote, CancellationReason, CancellationRequestStatus, CancellationRequestedAt, CancellationReviewedAt, IsPreparing, IsReadyForPickup, PreparingAt, ReadyForPickupAt, ReturnedAt, ReturnedByRiderId, ReturnedByRiderId1) VALUES
(5, '0623d7d6-1ee4-484d-b647-b53df173fb38', 'Lot 576, Bago Village, Bago Gallera, City of Davao, Davao Del Sur, Landmark: near hell', 2250.000000000000000000000000000000, 'Returns', '2026-05-02 00:52:29.307553', '09123456789', 'Robi Ali', 0.000000000000000000000000000000, '2026-05-08 23:51:36.357360', '2026-05-08 23:24:44.493286', NULL, 'Returned to Seller', '2026-05-09 00:27:12.038230', NULL, '2026-05-09 02:41:59.111819', 'Failed delivery attempt', '2026-05-14 02:41:59.112342', 'Cancellation approved. Order will be returned to seller.', 'Buyer requested cancellation while order is already in delivery process.', 'Approved', '2026-05-09 02:42:26.969350', '2026-05-09 02:42:45.817694', 0, 0, NULL, NULL, NULL, NULL, NULL),
(6, '31a998f2-b606-4aca-8f70-0adc3e8b53bc', 'Lot 15, Street 105, Baliok, City of Davao, Davao Del Sur, Landmark: Near Church', 6750.000000000000000000000000000000, 'To Receive', '2026-05-09 02:06:13.792926', '09123465555', 'Ezekiel Pol', 0.000000000000000000000000000000, '2026-05-13 15:15:11.535155', '2026-05-13 15:14:51.011773', NULL, 'Failed Delivery', '2026-05-13 15:15:17.273818', '15fec38d-d2dd-442c-b232-844e745235e4', '2026-05-13 15:19:59.117184', 'Failed delivery attempt', '2026-05-18 15:15:22.878885', NULL, NULL, NULL, NULL, NULL, 1, 1, '2026-05-09 02:09:45.480055', '2026-05-09 02:10:32.032693', NULL, NULL, NULL),
(7, '988a899f-5bab-4e80-a27d-d82206c2be3a', '56, Mabini Street, Callao, Gonzaga, Cagayan', 560.000000000000000000000000000000, 'Ready for Pickup', '2026-05-11 01:05:37.419753', '09271555222', 'Lava Hings', 60.000000000000000000000000000000, NULL, NULL, NULL, 'Pending Assignment', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, 1, '2026-05-11 01:26:37.729128', '2026-05-11 01:26:40.117746', NULL, NULL, NULL),
(9, '31a998f2-b606-4aca-8f70-0adc3e8b53bc', '156, Tokyo Street, Bago Gallera, City of Davao, Davao Del Sur', 560.000000000000000000000000000000, 'Delivered', '2026-05-15 01:40:53.415893', '09123745535', 'Ezekiel Pol', 60.000000000000000000000000000000, '2026-05-15 02:21:53.911231', '2026-05-15 02:21:38.854097', '2026-05-15 02:28:35.401879', 'Delivered', '2026-05-15 02:28:32.283882', '15fec38d-d2dd-442c-b232-844e745235e4', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, 1, '2026-05-15 02:18:55.453395', '2026-05-15 02:19:03.941308', NULL, NULL, NULL),
(10, '988a899f-5bab-4e80-a27d-d82206c2be3a', '156, Tokyo Street, Daliao, City of Davao, Davao Del Sur', 2310.000000000000000000000000000000, 'To Ship', '2026-05-15 15:36:28.580935', '09123745535', 'Lava Hings', 60.000000000000000000000000000000, NULL, NULL, NULL, 'Pending Assignment', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, NULL, NULL, NULL, NULL, NULL),
(15, '988a899f-5bab-4e80-a27d-d82206c2be3a', '156, Tokyo Street, Daliao, City of Davao, Davao Del Sur', 2310.000000000000000000000000000000, 'To Review', '2026-05-15 15:45:56.663955', '09123745535', 'Lava Hings', 60.000000000000000000000000000000, '2026-05-15 15:59:40.334811', '2026-05-15 15:59:30.689805', '2026-05-15 15:59:45.096227', 'Delivered', '2026-05-15 15:59:42.040839', '15fec38d-d2dd-442c-b232-844e745235e4', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, 1, '2026-05-15 15:59:03.484879', '2026-05-15 15:59:05.712296', NULL, NULL, NULL),
(17, '988a899f-5bab-4e80-a27d-d82206c2be3a', '156, Tokyo Street, Bangkas Heights, City of Davao, Davao Del Sur', 6750.000000000000000000000000000000, 'Pending Payment', '2026-05-15 16:01:25.967932', '09123745535', 'Lava Hings', 0.000000000000000000000000000000, NULL, NULL, NULL, 'Pending Assignment', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, NULL, NULL, NULL, NULL, NULL),
(18, '988a899f-5bab-4e80-a27d-d82206c2be3a', '156, Tokyo Street, Bangkas Heights, City of Davao, Davao Del Sur', 6750.000000000000000000000000000000, 'To Review', '2026-05-15 16:03:19.861634', '09123745535', 'Lava Hings', 0.000000000000000000000000000000, '2026-05-15 16:04:42.013900', '2026-05-15 16:04:35.800314', '2026-05-15 16:05:01.276011', 'Delivered', '2026-05-15 16:04:56.159838', '15fec38d-d2dd-442c-b232-844e745235e4', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, 1, '2026-05-15 16:04:11.835484', '2026-05-15 16:04:13.799665', NULL, NULL, NULL),
(19, '988a899f-5bab-4e80-a27d-d82206c2be3a', '7565, Toto Street, Beckel, La Trinidad, Benguet', 6750.000000000000000000000000000000, 'To Ship', '2026-05-16 18:04:19.197035', '09182736633', 'Lava Hings', 0.000000000000000000000000000000, NULL, NULL, NULL, 'Pending Assignment', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0, NULL, NULL, NULL, NULL, NULL);

-- --------------------------------------------------------

--
-- Table structure for table payments
--

CREATE TABLE payments (
  Id int(11) NOT NULL,
  OrderId int(11) NOT NULL,
  Amount decimal(18,2) NOT NULL,
  PaymentMethod varchar(50) NOT NULL,
  PaymentStatus varchar(50) NOT NULL,
  ReferenceNumber varchar(100) DEFAULT NULL,
  CreatedAt datetime(6) NOT NULL,
  PaidAt datetime(6) DEFAULT NULL,
  IsRefunded tinyint(1) NOT NULL,
  RefundAmount decimal(18,2) DEFAULT NULL,
  RefundedAt datetime(6) DEFAULT NULL,
  RefundReason varchar(500) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table payments
--

INSERT INTO payments (Id, OrderId, Amount, PaymentMethod, PaymentStatus, ReferenceNumber, CreatedAt, PaidAt, IsRefunded, RefundAmount, RefundedAt, RefundReason) VALUES
(1, 7, 560.00, 'COD', 'Pending', 'PAY-20260511010537-7', '2026-05-11 01:05:37.634475', NULL, 0, NULL, NULL, NULL),
(3, 9, 560.00, 'PayMongo', 'Paid', 'PAY-20260515014053-9', '2026-05-15 01:40:53.638581', '2026-05-15 01:41:51.780249', 0, NULL, NULL, NULL),
(4, 10, 2310.00, 'PayMongo', 'Pending Payment', 'PAY-20260515153628-10', '2026-05-15 15:36:28.766903', NULL, 0, NULL, NULL, NULL),
(9, 15, 2310.00, 'COD', 'Paid', 'PAY-20260515154556-15', '2026-05-15 15:45:56.666943', '2026-05-15 15:59:45.100345', 0, NULL, NULL, NULL),
(11, 17, 6750.00, 'PayMongo', 'Pending Payment', 'PAY-20260515160125-17', '2026-05-15 16:01:25.973739', NULL, 0, NULL, NULL, NULL),
(12, 18, 6750.00, 'PayMongo', 'Paid', 'PAY-20260515160319-18', '2026-05-15 16:03:19.873722', '2026-05-15 16:03:52.816705', 0, NULL, NULL, NULL),
(13, 19, 6750.00, 'PayMongo', 'Paid', 'PAY-20260516180419-19', '2026-05-16 18:04:19.380387', '2026-05-16 18:05:27.569462', 0, NULL, NULL, NULL);

-- --------------------------------------------------------

--
-- Table structure for table paymenttransactions
--

CREATE TABLE paymenttransactions (
  Id int(11) NOT NULL,
  PaymentId int(11) NOT NULL,
  OrderId int(11) NOT NULL,
  Provider longtext NOT NULL,
  ProviderReferenceId longtext DEFAULT NULL,
  CheckoutUrl longtext DEFAULT NULL,
  Status longtext NOT NULL,
  Amount decimal(18,2) NOT NULL,
  RawResponse longtext DEFAULT NULL,
  CreatedAt datetime(6) NOT NULL,
  PaidAt datetime(6) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table paymongowebhooklogs
--

CREATE TABLE paymongowebhooklogs (
  Id int(11) NOT NULL,
  EventType varchar(100) DEFAULT NULL,
  OrderId int(11) DEFAULT NULL,
  Status varchar(50) NOT NULL,
  Payload longtext DEFAULT NULL,
  Message longtext DEFAULT NULL,
  CreatedAt datetime(6) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table platformearnings
--

CREATE TABLE platformearnings (
  Id int(11) NOT NULL,
  OrderId int(11) NOT NULL,
  ProductTotal decimal(18,2) NOT NULL,
  CommissionAmount decimal(18,2) NOT NULL,
  CommissionRate decimal(18,2) NOT NULL,
  CreatedAt datetime(6) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table platformearnings
--

INSERT INTO platformearnings (Id, OrderId, ProductTotal, CommissionAmount, CommissionRate, CreatedAt) VALUES
(1, 9, 500.00, 50.00, 10.00, '2026-05-15 02:28:35.485580'),
(2, 15, 2250.00, 225.00, 10.00, '2026-05-15 15:59:45.174735'),
(3, 18, 6750.00, 675.00, 10.00, '2026-05-15 16:03:52.840755'),
(4, 19, 6750.00, 675.00, 10.00, '2026-05-16 18:05:27.651983');

-- --------------------------------------------------------

--
-- Table structure for table productimages
--

CREATE TABLE productimages (
  Id int(11) NOT NULL,
  ImagePath longtext NOT NULL,
  ProductId int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table productimages
--

INSERT INTO productimages (Id, ImagePath, ProductId) VALUES
(1, '/uploads/products/91b12af0-62e5-4747-9341-2d20f50d4e7d.png', 3),
(2, '/uploads/products/ceccbe00-00d6-4aa2-a35a-b54741cfbef7.png', 3),
(3, '/uploads/products/f6c7ed67-d264-4bd4-82de-33e9f603226b.jpg', 4),
(4, '/uploads/products/657f3c9a-61bc-4396-89d2-13e7356f5ad3.jpg', 4);

-- --------------------------------------------------------

--
-- Table structure for table products
--

CREATE TABLE products (
  Id int(11) NOT NULL,
  Name varchar(100) NOT NULL,
  Price decimal(18,2) NOT NULL,
  Stock int(11) NOT NULL,
  Category varchar(100) NOT NULL DEFAULT '',
  CreatedAt datetime(6) NOT NULL DEFAULT '0001-01-01 00:00:00.000000',
  Description varchar(500) NOT NULL DEFAULT '',
  SellerId varchar(255) NOT NULL DEFAULT '',
  UpdatedAt datetime(6) DEFAULT NULL,
  AvailableVariants varchar(500) DEFAULT NULL,
  AvailableSizes varchar(500) DEFAULT NULL,
  AdminHiddenReason longtext DEFAULT NULL,
  HiddenAt datetime(6) DEFAULT NULL,
  IsHiddenByAdmin tinyint(1) NOT NULL DEFAULT 0
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table products
--

INSERT INTO products (Id, Name, Price, Stock, Category, CreatedAt, Description, SellerId, UpdatedAt, AvailableVariants, AvailableSizes, AdminHiddenReason, HiddenAt, IsHiddenByAdmin) VALUES
(1, 'test product', 500.00, 23, 'Electronics', '2026-04-23 02:23:49.147623', 'just a first test product random okay', '0623d7d6-1ee4-484d-b647-b53df173fb38', NULL, NULL, NULL, NULL, NULL, 0),
(3, 'Haylou Watch', 2250.00, 9, 'Electronics', '2026-04-29 00:24:38.100174', 'Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry\'s standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book', '0623d7d6-1ee4-484d-b647-b53df173fb38', '2026-05-01 01:00:28.549230', 'Black, White, Army Green', NULL, NULL, NULL, 0),
(4, 'Fear of God ESSENTIALS', 6750.00, 10, 'Clothing', '2026-05-01 01:39:59.417382', 'These tees are perfect for pairing with sweatpants, jeans, or jackets, making them a versatile addition to your wardrobe. \r\nFor more details, you can explore the latest collection on the official Fear of God Essentials website or other retailers.', '0623d7d6-1ee4-484d-b647-b53df173fb38', '2026-05-15 16:08:15.055270', 'Beige, White ', 'S, M, L, XL, XXL', NULL, NULL, 0);

-- --------------------------------------------------------

--
-- Table structure for table refunds
--

CREATE TABLE refunds (
  Id int(11) NOT NULL,
  OrderId int(11) NOT NULL,
  PaymentId int(11) NOT NULL,
  Amount decimal(18,2) NOT NULL,
  Reason longtext NOT NULL,
  Status longtext NOT NULL,
  RequestedAt datetime(6) NOT NULL,
  ReviewedAt datetime(6) DEFAULT NULL,
  AdminNote longtext DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table riderprofiles
--

CREATE TABLE riderprofiles (
  Id int(11) NOT NULL,
  UserId varchar(255) NOT NULL,
  FullName longtext NOT NULL,
  PhoneNumber longtext NOT NULL,
  AssignedLocation longtext NOT NULL,
  VehicleType longtext DEFAULT NULL,
  Status longtext NOT NULL,
  CreatedAt datetime(6) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table riderprofiles
--

INSERT INTO riderprofiles (Id, UserId, FullName, PhoneNumber, AssignedLocation, VehicleType, Status, CreatedAt) VALUES
(1, '8048a2ad-9b70-4ddc-ae4a-49004f717b24', 'Luwi Paloso', '09123467777', 'Street 101, Talibong, Cabucgayan, Biliran', 'Motorcyle', 'PendingPasswordSetup', '2026-05-08 18:46:06.665177'),
(2, 'e2f8c79e-49c8-42eb-8a2f-99db385d2b06', 'Ren Niel ', '09123456780', 'Street 105, San Juan, Hagonoy, Bulacan', 'Motorcyle', 'Active', '2026-05-08 19:43:04.513682'),
(3, '15fec38d-d2dd-442c-b232-844e745235e4', 'Neo Kenneth', '09770795986', 'Street 111, Daliao, City of Davao, Davao Del Sur', 'Motorcyle', 'Active', '2026-05-08 23:23:36.927910');

-- --------------------------------------------------------

--
-- Table structure for table sellerapplicationdocument
--

CREATE TABLE sellerapplicationdocument (
  Id int(11) NOT NULL,
  SellerApplicationId int(11) NOT NULL,
  FileName longtext NOT NULL,
  ContentType longtext NOT NULL,
  FileSize bigint(20) NOT NULL,
  UploadedAt datetime(6) NOT NULL,
  FilePath longtext NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table sellerapplicationdocument
--

INSERT INTO sellerapplicationdocument (Id, SellerApplicationId, FileName, ContentType, FileSize, UploadedAt, FilePath) VALUES
(1, 4, 'IT15-DELIVERABLES1-FINALDOCS.pdf', 'application/pdf', 57420, '2026-05-03 00:57:10.604181', ''),
(2, 4, 'CCE106L_CAMPUS CHRONICLE_ERD.drawio.png', 'image/png', 193136, '2026-05-03 00:57:10.660330', ''),
(3, 6, 'AIM LOGO.jpg', 'image/jpeg', 102967, '2026-05-08 14:06:36.741884', '/uploads/seller-documents/381c1496-2d6a-47db-b77b-c0f6b12b9969.jpg'),
(4, 6, '7dd912ca-9ff5-4ce8-aef3-0d9526698be8.pdf', 'application/pdf', 2551209, '2026-05-08 14:06:36.747653', '/uploads/seller-documents/758630ae-e48a-4fa6-80d8-43edbee5984e.pdf'),
(5, 6, '488788375-5420-1604328865.pdf', 'application/pdf', 162132, '2026-05-08 14:06:36.748603', '/uploads/seller-documents/f486204e-6065-4d2a-af6f-ab80e6d8173c.pdf');

-- --------------------------------------------------------

--
-- Table structure for table sellerapplications
--

CREATE TABLE sellerapplications (
  Id int(11) NOT NULL,
  UserId varchar(255) NOT NULL,
  FullName varchar(100) NOT NULL,
  ShopName varchar(100) NOT NULL,
  PhoneNumber longtext NOT NULL,
  Address varchar(200) NOT NULL,
  BusinessDescription varchar(500) NOT NULL,
  Status longtext NOT NULL,
  CreatedAt datetime(6) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table sellerapplications
--

INSERT INTO sellerapplications (Id, UserId, FullName, ShopName, PhoneNumber, Address, BusinessDescription, Status, CreatedAt) VALUES
(1, 'e20f4d92-3908-4527-a13c-f3a6ccb769a2', 'test 101', 'test101seller', '09999999999', 'Malvar Street, Davao City ', 'just a simple business test seller 101', 'Approved', '2026-04-16 02:25:56.745097'),
(2, '0623d7d6-1ee4-484d-b647-b53df173fb38', 'Jieun', 'JIEUN IU SHOP', '09123456789', 'Baguio City District 101 Test', 'shop of iu my idol', 'Approved', '2026-04-23 00:42:44.942179'),
(3, '31a998f2-b606-4aca-8f70-0adc3e8b53bc', 'Ezekiel Pol', 'PetCo', '09123456789', 'Lot 576, Labaan, Bucloc, Abra', 'Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. ', 'Cancelled', '2026-05-02 20:48:58.148979'),
(4, '31a998f2-b606-4aca-8f70-0adc3e8b53bc', 'Ezekiel Pol', 'PetCo', '09123456789', 'Lot 576, Collago, Lagayan, Abra', 'Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.', 'Pending', '2026-05-03 00:57:10.535206'),
(6, '988a899f-5bab-4e80-a27d-d82206c2be3a', 'Lava Hings', 'H20 Clothing', '09123456780', 'Street 101, Silo-o, Malitbog, Bukidnon', 'Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur.', 'Pending', '2026-05-08 14:06:36.738165');

-- --------------------------------------------------------

--
-- Table structure for table sellerpayouts
--

CREATE TABLE sellerpayouts (
  Id int(11) NOT NULL,
  SellerId varchar(255) NOT NULL,
  OrderId int(11) NOT NULL,
  ProductTotal decimal(18,2) NOT NULL,
  CommissionAmount decimal(18,2) NOT NULL,
  SellerEarnings decimal(18,2) NOT NULL,
  CommissionRate decimal(18,2) NOT NULL,
  Status longtext NOT NULL,
  CreatedAt datetime(6) NOT NULL,
  ReleasedAt datetime(6) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table sellerpayouts
--

INSERT INTO sellerpayouts (Id, SellerId, OrderId, ProductTotal, CommissionAmount, SellerEarnings, CommissionRate, Status, CreatedAt, ReleasedAt) VALUES
(1, '0623d7d6-1ee4-484d-b647-b53df173fb38', 9, 500.00, 50.00, 450.00, 10.00, 'Released', '2026-05-15 02:28:35.465165', '2026-05-15 02:41:53.359794'),
(2, '0623d7d6-1ee4-484d-b647-b53df173fb38', 15, 2250.00, 225.00, 2025.00, 10.00, 'Released', '2026-05-15 15:59:45.153990', '2026-05-15 16:07:21.166934'),
(3, '0623d7d6-1ee4-484d-b647-b53df173fb38', 18, 6750.00, 675.00, 6075.00, 10.00, 'Released', '2026-05-15 16:03:52.840656', '2026-05-15 16:07:22.044351'),
(4, '0623d7d6-1ee4-484d-b647-b53df173fb38', 19, 6750.00, 675.00, 6075.00, 10.00, 'Pending', '2026-05-16 18:05:27.632582', NULL);

-- --------------------------------------------------------

--
-- Table structure for table shopprofiles
--

CREATE TABLE shopprofiles (
  Id int(11) NOT NULL,
  SellerId varchar(255) NOT NULL,
  ShopName varchar(100) NOT NULL,
  ShopDescription varchar(500) DEFAULT NULL,
  BusinessEmail varchar(150) DEFAULT NULL,
  BusinessPhone varchar(30) DEFAULT NULL,
  ShopAddress varchar(300) DEFAULT NULL,
  City varchar(100) DEFAULT NULL,
  Province varchar(100) DEFAULT NULL,
  PostalCode varchar(20) DEFAULT NULL,
  BusinessHours varchar(100) DEFAULT NULL,
  ReturnPolicy varchar(100) DEFAULT NULL,
  LogoPath longtext DEFAULT NULL,
  CreatedAt datetime(6) NOT NULL,
  UpdatedAt datetime(6) DEFAULT NULL,
  Barangay varchar(100) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table shopprofiles
--

INSERT INTO shopprofiles (Id, SellerId, ShopName, ShopDescription, BusinessEmail, BusinessPhone, ShopAddress, City, Province, PostalCode, BusinessHours, ReturnPolicy, LogoPath, CreatedAt, UpdatedAt, Barangay) VALUES
(1, '0623d7d6-1ee4-484d-b647-b53df173fb38', 'IU\'s Shop', 'But I must explain to you how all this mistaken idea of denouncing pleasure and praising pain was born and I will give you a complete account of the system, and expound the actual teachings of the great explorer of the truth, the master-builder of human happiness.', 'iujieun@gmail.com', '09123456789', 'Street 101, Brgy. Toril, Alburquerque, Bohol', 'Alburquerque', 'Bohol', '900', NULL, 'N/A iskamas', '/uploads/shops/58ac3f6c-d72a-4abd-bc8d-48c5c5f56388.jpg', '2026-05-06 23:27:13.717436', '2026-05-06 23:49:13.123688', 'Toril');

-- --------------------------------------------------------

--
-- Table structure for table superadminauditlogs
--

CREATE TABLE superadminauditlogs (
  Id int(11) NOT NULL,
  SuperAdminId varchar(255) NOT NULL,
  Action longtext NOT NULL,
  EntityType longtext NOT NULL,
  EntityId longtext NOT NULL,
  Description longtext DEFAULT NULL,
  CreatedAt datetime(6) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table superadminauditlogs
--

INSERT INTO superadminauditlogs (Id, SuperAdminId, Action, EntityType, EntityId, Description, CreatedAt) VALUES
(1, '07cd35c6-fce8-4823-9a76-00c69ad7ba19', 'Activate User', 'User', '8048a2ad-9b70-4ddc-ae4a-49004f717b24', 'Activated user account Luwi Paloso (kirbykiero@gmail.com).', '2026-05-15 04:29:01.103958'),
(2, '07cd35c6-fce8-4823-9a76-00c69ad7ba19', 'Test Audit Log', 'System', 'TEST', 'This is a test audit log created from the SuperAdmin Audit Logs page.', '2026-05-15 04:29:04.822362'),
(3, '07cd35c6-fce8-4823-9a76-00c69ad7ba19', 'Deactivate User', 'User', '8048a2ad-9b70-4ddc-ae4a-49004f717b24', 'Deactivated user account Luwi Paloso (kirbykiero@gmail.com).', '2026-05-15 04:29:13.260617'),
(4, '07cd35c6-fce8-4823-9a76-00c69ad7ba19', 'Activate User', 'User', '8048a2ad-9b70-4ddc-ae4a-49004f717b24', 'Activated user account Luwi Paloso (kirbykiero@gmail.com).', '2026-05-15 04:31:41.470360'),
(5, '07cd35c6-fce8-4823-9a76-00c69ad7ba19', 'Deactivate User', 'User', '8048a2ad-9b70-4ddc-ae4a-49004f717b24', 'Deactivated user account Luwi Paloso (kirbykiero@gmail.com).', '2026-05-15 04:31:52.644298'),
(6, '07cd35c6-fce8-4823-9a76-00c69ad7ba19', 'Activate User', 'User', '8048a2ad-9b70-4ddc-ae4a-49004f717b24', 'Activated user account Luwi Paloso (kirbykiero@gmail.com).', '2026-05-15 04:33:34.659838'),
(7, '07cd35c6-fce8-4823-9a76-00c69ad7ba19', 'Deactivate User', 'User', '8048a2ad-9b70-4ddc-ae4a-49004f717b24', 'Deactivated user account Luwi Paloso (kirbykiero@gmail.com).', '2026-05-15 04:35:33.628491'),
(8, '07cd35c6-fce8-4823-9a76-00c69ad7ba19', 'Activate User', 'User', '8048a2ad-9b70-4ddc-ae4a-49004f717b24', 'Activated user account Luwi Paloso (kirbykiero@gmail.com).', '2026-05-15 04:36:08.320774'),
(9, '07cd35c6-fce8-4823-9a76-00c69ad7ba19', 'Test Audit Log', 'System', 'TEST', 'This is a test audit log created from the SuperAdmin Audit Logs page.', '2026-05-15 04:36:21.477519'),
(10, '07cd35c6-fce8-4823-9a76-00c69ad7ba19', 'Test Audit Log', 'System', 'TEST', 'This is a test audit log created from the SuperAdmin Audit Logs page.', '2026-05-15 04:36:22.267097'),
(11, '07cd35c6-fce8-4823-9a76-00c69ad7ba19', 'Test Audit Log', 'System', 'TEST', 'This is a test audit log created from the SuperAdmin Audit Logs page.', '2026-05-15 04:36:22.720296'),
(12, '07cd35c6-fce8-4823-9a76-00c69ad7ba19', 'Test Audit Log', 'System', 'TEST', 'This is a test audit log created from the SuperAdmin Audit Logs page.', '2026-05-15 04:36:22.892638'),
(13, '07cd35c6-fce8-4823-9a76-00c69ad7ba19', 'Deactivate User', 'User', '8048a2ad-9b70-4ddc-ae4a-49004f717b24', 'Deactivated user account Luwi Paloso (kirbykiero@gmail.com).', '2026-05-15 04:36:56.566289'),
(14, '07cd35c6-fce8-4823-9a76-00c69ad7ba19', 'Activate User', 'User', '8048a2ad-9b70-4ddc-ae4a-49004f717b24', 'Activated user account Luwi Paloso (kirbykiero@gmail.com).', '2026-05-15 04:37:03.055955'),
(15, '07cd35c6-fce8-4823-9a76-00c69ad7ba19', 'Deactivate User', 'User', '8048a2ad-9b70-4ddc-ae4a-49004f717b24', 'Deactivated user account Luwi Paloso (kirbykiero@gmail.com).', '2026-05-15 04:38:30.526801'),
(16, '07cd35c6-fce8-4823-9a76-00c69ad7ba19', 'Activate User', 'User', '8048a2ad-9b70-4ddc-ae4a-49004f717b24', 'Activated user account Luwi Paloso (kirbykiero@gmail.com).', '2026-05-15 04:38:32.666003'),
(17, '07cd35c6-fce8-4823-9a76-00c69ad7ba19', 'Deactivate Seller', 'Seller', '0623d7d6-1ee4-484d-b647-b53df173fb38', 'Deactivated seller account Jieun (iujieungaming@gmail.com).', '2026-05-15 04:38:39.211300'),
(18, '07cd35c6-fce8-4823-9a76-00c69ad7ba19', 'Activate Seller', 'Seller', '0623d7d6-1ee4-484d-b647-b53df173fb38', 'Activated seller account Jieun (iujieungaming@gmail.com).', '2026-05-15 04:38:49.041353'),
(19, '07cd35c6-fce8-4823-9a76-00c69ad7ba19', 'Test Audit Log', 'System', 'TEST', 'This is a test audit log created from the SuperAdmin Audit Logs page.', '2026-05-15 04:40:46.799190'),
(20, '07cd35c6-fce8-4823-9a76-00c69ad7ba19', 'Test Audit Log', 'System', 'TEST', 'This is a test audit log created from the SuperAdmin Audit Logs page.', '2026-05-15 04:40:47.761950'),
(21, '07cd35c6-fce8-4823-9a76-00c69ad7ba19', 'Test Audit Log', 'System', 'TEST', 'This is a test audit log created from the SuperAdmin Audit Logs page.', '2026-05-15 04:40:49.030486'),
(22, '07cd35c6-fce8-4823-9a76-00c69ad7ba19', 'Deactivate User', 'User', '8048a2ad-9b70-4ddc-ae4a-49004f717b24', 'Deactivated user account Luwi Paloso (kirbykiero@gmail.com).', '2026-05-15 04:40:56.741376'),
(23, '07cd35c6-fce8-4823-9a76-00c69ad7ba19', 'Test Audit Log', 'System', 'TEST', 'This is a test audit log created from the SuperAdmin Audit Logs page.', '2026-05-15 13:53:23.505722'),
(24, '07cd35c6-fce8-4823-9a76-00c69ad7ba19', 'Deactivate Admin', 'Admin', 'e510af4e-e904-451b-a260-a3eff3ab9387', 'Deactivated admin account Sync Kit (synckitvx@gmail.com).', '2026-05-15 15:01:53.295330'),
(25, '07cd35c6-fce8-4823-9a76-00c69ad7ba19', 'Activate Admin', 'Admin', 'e510af4e-e904-451b-a260-a3eff3ab9387', 'Activated admin account Sync Kit (synckitvx@gmail.com).', '2026-05-15 15:02:03.049230'),
(26, '07cd35c6-fce8-4823-9a76-00c69ad7ba19', 'Activate User', 'User', '8048a2ad-9b70-4ddc-ae4a-49004f717b24', 'Activated user account Luwi Paloso (kirbykiero@gmail.com).', '2026-05-15 15:25:48.891339'),
(27, '07cd35c6-fce8-4823-9a76-00c69ad7ba19', 'Deactivate User', 'User', '8048a2ad-9b70-4ddc-ae4a-49004f717b24', 'Deactivated user account Luwi Paloso (kirbykiero@gmail.com).', '2026-05-15 15:25:52.099305'),
(28, '07cd35c6-fce8-4823-9a76-00c69ad7ba19', 'Release Settlement', 'Seller Settlement', '2', 'Released settlement for Order #15. Seller: iujieungaming@gmail.com. Net earnings: ₱2,025.00.', '2026-05-15 16:07:21.172194'),
(29, '07cd35c6-fce8-4823-9a76-00c69ad7ba19', 'Release Settlement', 'Seller Settlement', '3', 'Released settlement for Order #18. Seller: iujieungaming@gmail.com. Net earnings: ₱6,075.00.', '2026-05-15 16:07:22.046377');

-- --------------------------------------------------------

--
-- Table structure for table systemsettings
--

CREATE TABLE systemsettings (
  Id int(11) NOT NULL,
  AllowShopperRegistration tinyint(1) NOT NULL,
  AllowSellerRegistration tinyint(1) NOT NULL,
  AllowRiderRegistration tinyint(1) NOT NULL,
  UpdatedAt datetime(6) NOT NULL,
  MaintenanceMessage longtext DEFAULT NULL,
  MaintenanceMode tinyint(1) NOT NULL DEFAULT 0
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table systemsettings
--

INSERT INTO systemsettings (Id, AllowShopperRegistration, AllowSellerRegistration, AllowRiderRegistration, UpdatedAt, MaintenanceMessage, MaintenanceMode) VALUES
(1, 1, 1, 1, '2026-05-14 23:45:25.854130', NULL, 0);

-- --------------------------------------------------------

--
-- Table structure for table __efmigrationshistory
--

CREATE TABLE __efmigrationshistory (
  MigrationId varchar(150) NOT NULL,
  ProductVersion varchar(32) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table __efmigrationshistory
--

INSERT INTO __efmigrationshistory (MigrationId, ProductVersion) VALUES
('20260407021100_InitialCreate', '9.0.0'),
('20260407024631_AddIdentity', '9.0.0'),
('20260413164955_AddSellerApplication', '9.0.0'),
('20260418093252_AddVerificationFields', '9.0.0'),
('20260422175827_AddProductsTable', '9.0.0'),
('20260428155929_AddProductImages', '9.0.0'),
('20260430164755_AddAvailableVariantsToProduct', '9.0.0'),
('20260430192321_AddCartItemsTable', '9.0.0'),
('20260501135114_AddOrdersAndOrderItems', '9.0.0'),
('20260501164257_AddReceiverDetailsToOrders', '9.0.0'),
('20260502162502_AddSellerApplicationDocuments', '9.0.0'),
('20260505094613_AddShippingFeeToOrders', '9.0.0'),
('20260506151737_AddShopProfile', '9.0.0'),
('20260506154511_AddBarangayToShopProfile', '9.0.0'),
('20260508060537_UpdateSellerDocuments', '9.0.0'),
('20260508083809_AddRiderProfiles', '9.0.0'),
('20260508115455_AddRiderDeliveryFieldsToOrders', '9.0.0'),
('20260508160514_AddFailedDeliveryFieldsToOrders', '9.0.0'),
('20260508170459_AddOrderCancellationRequestFields', '9.0.0'),
('20260508173723_AddSellerFulfillmentFieldsToOrders', '9.0.0'),
('20260509180109_AddAdminProductModeration', '9.0.0'),
('20260510145421_AddAuditLogs', '9.0.0'),
('20260510161738_AddPayments', '9.0.0'),
('20260513070123_AddReturnedByRiderToOrders', '9.0.0'),
('20260514065836_AddShopNameToApplicationUser', '9.0.0'),
('20260514153928_AddSystemSettings', '9.0.0'),
('20260514170037_AddMaintenanceModeSettings', '9.0.0'),
('20260514175047_AddMarketplaceFinancialTables', '9.0.0'),
('20260514193849_AddPayMongoWebhookLogs', '9.0.0'),
('20260514202820_AddSuperAdminAuditLogs', '9.0.0');

--
-- Indexes for dumped tables
--

--
-- Indexes for table aspnetroleclaims
--
ALTER TABLE aspnetroleclaims
  ADD KEY IX_AspNetRoleClaims_RoleId (RoleId);

--
-- Indexes for table aspnetroles
--
ALTER TABLE aspnetroles
  ADD PRIMARY KEY (Id),
  ADD UNIQUE KEY RoleNameIndex (NormalizedName);

--
-- Indexes for table aspnetuserclaims
--
ALTER TABLE aspnetuserclaims
  ADD PRIMARY KEY (Id),
  ADD KEY IX_AspNetUserClaims_UserId (UserId);

--
-- Indexes for table aspnetuserlogins
--
ALTER TABLE aspnetuserlogins
  ADD PRIMARY KEY (LoginProvider,ProviderKey),
  ADD KEY IX_AspNetUserLogins_UserId (UserId);

--
-- Indexes for table aspnetuserroles
--
ALTER TABLE aspnetuserroles
  ADD PRIMARY KEY (UserId,RoleId),
  ADD KEY IX_AspNetUserRoles_RoleId (RoleId);

--
-- Indexes for table aspnetusers
--
ALTER TABLE aspnetusers
  ADD PRIMARY KEY (Id),
  ADD UNIQUE KEY UserNameIndex (NormalizedUserName),
  ADD KEY EmailIndex (NormalizedEmail);

--
-- Indexes for table aspnetusertokens
--
ALTER TABLE aspnetusertokens
  ADD PRIMARY KEY (UserId,LoginProvider,Name);

--
-- Indexes for table auditlogs
--
ALTER TABLE auditlogs
  ADD PRIMARY KEY (Id),
  ADD KEY IX_AuditLogs_AdminId (AdminId);

--
-- Indexes for table cartitems
--
ALTER TABLE cartitems
  ADD PRIMARY KEY (Id),
  ADD KEY IX_CartItems_ProductId (ProductId),
  ADD KEY IX_CartItems_ShopperId (ShopperId);

--
-- Indexes for table categories
--
ALTER TABLE categories
  ADD PRIMARY KEY (Id);

--
-- Indexes for table commissionrates
--
ALTER TABLE commissionrates
  ADD PRIMARY KEY (Id);

--
-- Indexes for table orderitems
--
ALTER TABLE orderitems
  ADD PRIMARY KEY (Id),
  ADD KEY IX_OrderItems_OrderId (OrderId),
  ADD KEY IX_OrderItems_ProductId (ProductId);

--
-- Indexes for table orders
--
ALTER TABLE orders
  ADD PRIMARY KEY (Id),
  ADD KEY IX_Orders_RiderId (RiderId),
  ADD KEY IX_Orders_ReturnedByRiderId1 (ReturnedByRiderId1);

--
-- Indexes for table payments
--
ALTER TABLE payments
  ADD PRIMARY KEY (Id),
  ADD KEY IX_Payments_OrderId (OrderId);

--
-- Indexes for table paymenttransactions
--
ALTER TABLE paymenttransactions
  ADD PRIMARY KEY (Id),
  ADD KEY IX_PaymentTransactions_OrderId (OrderId),
  ADD KEY IX_PaymentTransactions_PaymentId (PaymentId);

--
-- Indexes for table paymongowebhooklogs
--
ALTER TABLE paymongowebhooklogs
  ADD PRIMARY KEY (Id);

--
-- Indexes for table platformearnings
--
ALTER TABLE platformearnings
  ADD PRIMARY KEY (Id),
  ADD KEY IX_PlatformEarnings_OrderId (OrderId);

--
-- Indexes for table productimages
--
ALTER TABLE productimages
  ADD PRIMARY KEY (Id),
  ADD KEY IX_ProductImages_ProductId (ProductId);

--
-- Indexes for table products
--
ALTER TABLE products
  ADD PRIMARY KEY (Id),
  ADD KEY IX_Products_SellerId (SellerId);

--
-- Indexes for table refunds
--
ALTER TABLE refunds
  ADD PRIMARY KEY (Id),
  ADD KEY IX_Refunds_OrderId (OrderId),
  ADD KEY IX_Refunds_PaymentId (PaymentId);

--
-- Indexes for table riderprofiles
--
ALTER TABLE riderprofiles
  ADD PRIMARY KEY (Id),
  ADD KEY IX_RiderProfiles_UserId (UserId);

--
-- Indexes for table sellerapplicationdocument
--
ALTER TABLE sellerapplicationdocument
  ADD PRIMARY KEY (Id),
  ADD KEY IX_SellerApplicationDocument_SellerApplicationId (SellerApplicationId);

--
-- Indexes for table sellerapplications
--
ALTER TABLE sellerapplications
  ADD PRIMARY KEY (Id),
  ADD KEY IX_SellerApplications_UserId (UserId);

--
-- Indexes for table sellerpayouts
--
ALTER TABLE sellerpayouts
  ADD PRIMARY KEY (Id),
  ADD KEY IX_SellerPayouts_OrderId (OrderId),
  ADD KEY IX_SellerPayouts_SellerId (SellerId);

--
-- Indexes for table shopprofiles
--
ALTER TABLE shopprofiles
  ADD PRIMARY KEY (Id),
  ADD KEY IX_ShopProfiles_SellerId (SellerId);

--
-- Indexes for table superadminauditlogs
--
ALTER TABLE superadminauditlogs
  ADD PRIMARY KEY (Id),
  ADD KEY IX_SuperAdminAuditLogs_SuperAdminId (SuperAdminId);

--
-- Indexes for table systemsettings
--
ALTER TABLE systemsettings
  ADD PRIMARY KEY (Id);

--
-- Indexes for table __efmigrationshistory
--
ALTER TABLE __efmigrationshistory
  ADD PRIMARY KEY (MigrationId);

--
-- AUTO_INCREMENT for dumped tables
--

--
-- AUTO_INCREMENT for table aspnetroleclaims
--
ALTER TABLE aspnetroleclaims
  MODIFY Id int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table aspnetuserclaims
--
ALTER TABLE aspnetuserclaims
  MODIFY Id int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table auditlogs
--
ALTER TABLE auditlogs
  MODIFY Id int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table cartitems
--
ALTER TABLE cartitems
  MODIFY Id int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table categories
--
ALTER TABLE categories
  MODIFY Id int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table commissionrates
--
ALTER TABLE commissionrates
  MODIFY Id int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table orderitems
--
ALTER TABLE orderitems
  MODIFY Id int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table orders
--
ALTER TABLE orders
  MODIFY Id int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table payments
--
ALTER TABLE payments
  MODIFY Id int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table paymenttransactions
--
ALTER TABLE paymenttransactions
  MODIFY Id int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table paymongowebhooklogs
--
ALTER TABLE paymongowebhooklogs
  MODIFY Id int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table platformearnings
--
ALTER TABLE platformearnings
  MODIFY Id int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table productimages
--
ALTER TABLE productimages
  MODIFY Id int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table products
--
ALTER TABLE products
  MODIFY Id int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table refunds
--
ALTER TABLE refunds
  MODIFY Id int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table riderprofiles
--
ALTER TABLE riderprofiles
  MODIFY Id int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table sellerapplicationdocument
--
ALTER TABLE sellerapplicationdocument
  MODIFY Id int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table sellerapplications
--
ALTER TABLE sellerapplications
  MODIFY Id int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table sellerpayouts
--
ALTER TABLE sellerpayouts
  MODIFY Id int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table shopprofiles
--
ALTER TABLE shopprofiles
  MODIFY Id int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table superadminauditlogs
--
ALTER TABLE superadminauditlogs
  MODIFY Id int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table systemsettings
--
ALTER TABLE systemsettings
  MODIFY Id int(11) NOT NULL AUTO_INCREMENT;

--
-- Constraints for dumped tables
--

--
-- Constraints for table aspnetroleclaims
--
ALTER TABLE aspnetroleclaims
  ADD CONSTRAINT FK_AspNetRoleClaims_AspNetRoles_RoleId FOREIGN KEY (RoleId) REFERENCES aspnetroles (Id) ON DELETE CASCADE;

--
-- Constraints for table aspnetuserclaims
--
ALTER TABLE aspnetuserclaims
  ADD CONSTRAINT FK_AspNetUserClaims_AspNetUsers_UserId FOREIGN KEY (UserId) REFERENCES aspnetusers (Id) ON DELETE CASCADE;

--
-- Constraints for table aspnetuserlogins
--
ALTER TABLE aspnetuserlogins
  ADD CONSTRAINT FK_AspNetUserLogins_AspNetUsers_UserId FOREIGN KEY (UserId) REFERENCES aspnetusers (Id) ON DELETE CASCADE;

--
-- Constraints for table aspnetuserroles
--
ALTER TABLE aspnetuserroles
  ADD CONSTRAINT FK_AspNetUserRoles_AspNetRoles_RoleId FOREIGN KEY (RoleId) REFERENCES aspnetroles (Id) ON DELETE CASCADE,
  ADD CONSTRAINT FK_AspNetUserRoles_AspNetUsers_UserId FOREIGN KEY (UserId) REFERENCES aspnetusers (Id) ON DELETE CASCADE;

--
-- Constraints for table aspnetusertokens
--
ALTER TABLE aspnetusertokens
  ADD CONSTRAINT FK_AspNetUserTokens_AspNetUsers_UserId FOREIGN KEY (UserId) REFERENCES aspnetusers (Id) ON DELETE CASCADE;

--
-- Constraints for table auditlogs
--
ALTER TABLE auditlogs
  ADD CONSTRAINT FK_AuditLogs_AspNetUsers_AdminId FOREIGN KEY (AdminId) REFERENCES aspnetusers (Id) ON DELETE CASCADE;

--
-- Constraints for table cartitems
--
ALTER TABLE cartitems
  ADD CONSTRAINT FK_CartItems_AspNetUsers_ShopperId FOREIGN KEY (ShopperId) REFERENCES aspnetusers (Id) ON DELETE CASCADE,
  ADD CONSTRAINT FK_CartItems_Products_ProductId FOREIGN KEY (ProductId) REFERENCES products (Id) ON DELETE CASCADE;

--
-- Constraints for table orderitems
--
ALTER TABLE orderitems
  ADD CONSTRAINT FK_OrderItems_Orders_OrderId FOREIGN KEY (OrderId) REFERENCES `orders` (Id) ON DELETE CASCADE,
  ADD CONSTRAINT FK_OrderItems_Products_ProductId FOREIGN KEY (ProductId) REFERENCES products (Id) ON DELETE CASCADE;

--
-- Constraints for table orders
--
ALTER TABLE orders
  ADD CONSTRAINT FK_Orders_AspNetUsers_RiderId FOREIGN KEY (RiderId) REFERENCES aspnetusers (Id),
  ADD CONSTRAINT FK_Orders_RiderProfiles_ReturnedByRiderId1 FOREIGN KEY (ReturnedByRiderId1) REFERENCES riderprofiles (Id);

--
-- Constraints for table payments
--
ALTER TABLE payments
  ADD CONSTRAINT FK_Payments_Orders_OrderId FOREIGN KEY (OrderId) REFERENCES `orders` (Id) ON DELETE CASCADE;

--
-- Constraints for table paymenttransactions
--
ALTER TABLE paymenttransactions
  ADD CONSTRAINT FK_PaymentTransactions_Orders_OrderId FOREIGN KEY (OrderId) REFERENCES `orders` (Id) ON DELETE CASCADE,
  ADD CONSTRAINT FK_PaymentTransactions_Payments_PaymentId FOREIGN KEY (PaymentId) REFERENCES payments (Id) ON DELETE CASCADE;

--
-- Constraints for table platformearnings
--
ALTER TABLE platformearnings
  ADD CONSTRAINT FK_PlatformEarnings_Orders_OrderId FOREIGN KEY (OrderId) REFERENCES `orders` (Id) ON DELETE CASCADE;

--
-- Constraints for table productimages
--
ALTER TABLE productimages
  ADD CONSTRAINT FK_ProductImages_Products_ProductId FOREIGN KEY (ProductId) REFERENCES products (Id) ON DELETE CASCADE;

--
-- Constraints for table products
--
ALTER TABLE products
  ADD CONSTRAINT FK_Products_AspNetUsers_SellerId FOREIGN KEY (SellerId) REFERENCES aspnetusers (Id) ON DELETE CASCADE;

--
-- Constraints for table refunds
--
ALTER TABLE refunds
  ADD CONSTRAINT FK_Refunds_Orders_OrderId FOREIGN KEY (OrderId) REFERENCES `orders` (Id) ON DELETE CASCADE,
  ADD CONSTRAINT FK_Refunds_Payments_PaymentId FOREIGN KEY (PaymentId) REFERENCES payments (Id) ON DELETE CASCADE;

--
-- Constraints for table riderprofiles
--
ALTER TABLE riderprofiles
  ADD CONSTRAINT FK_RiderProfiles_AspNetUsers_UserId FOREIGN KEY (UserId) REFERENCES aspnetusers (Id) ON DELETE CASCADE;

--
-- Constraints for table sellerapplicationdocument
--
ALTER TABLE sellerapplicationdocument
  ADD CONSTRAINT FK_SellerApplicationDocument_SellerApplications_SellerApplicati~ FOREIGN KEY (SellerApplicationId) REFERENCES sellerapplications (Id) ON DELETE CASCADE;

--
-- Constraints for table sellerapplications
--
ALTER TABLE sellerapplications
  ADD CONSTRAINT FK_SellerApplications_AspNetUsers_UserId FOREIGN KEY (UserId) REFERENCES aspnetusers (Id) ON DELETE CASCADE;

--
-- Constraints for table sellerpayouts
--
ALTER TABLE sellerpayouts
  ADD CONSTRAINT FK_SellerPayouts_AspNetUsers_SellerId FOREIGN KEY (SellerId) REFERENCES aspnetusers (Id) ON DELETE CASCADE,
  ADD CONSTRAINT FK_SellerPayouts_Orders_OrderId FOREIGN KEY (OrderId) REFERENCES `orders` (Id) ON DELETE CASCADE;

--
-- Constraints for table shopprofiles
--
ALTER TABLE shopprofiles
  ADD CONSTRAINT FK_ShopProfiles_AspNetUsers_SellerId FOREIGN KEY (SellerId) REFERENCES aspnetusers (Id) ON DELETE CASCADE;

--
-- Constraints for table superadminauditlogs
--
ALTER TABLE superadminauditlogs
  ADD CONSTRAINT FK_SuperAdminAuditLogs_AspNetUsers_SuperAdminId FOREIGN KEY (SuperAdminId) REFERENCES aspnetusers (Id) ON DELETE CASCADE;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
