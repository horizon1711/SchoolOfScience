BEGIN TRANSACTION;

BEGIN TRY

--clear interview record
DELETE InterviewApplication;
DELETE Interview;

--clear appointment record
DELETE AppointmentAppointmentConcern;
DELETE AppointmentConcern;
DELETE Appointment;

--clear nomination record
DELETE Nomination;

--clear notification record
DELETE NotificationRecipient;
DELETE Notification;

--clear application record
DELETE ApplicationAttachment;
DELETE ApplicationExchangeOption;
DELETE ApplicationOptionValue;
DELETE Application;

--clear exchange option list
--DELETE ExchangeOption;

--clear student record
--DELETE StudentAdvisingRemark;
--DELETE StudentAdvisor;
--DELETE StudentExperience;
--DELETE StudentParticular;
--DELETE StudentQualification;
--DELETE StudentCourseHistory;
--DELETE StudentPublicExam;
--DELETE StudentTerm;
--DELETE StudentAcademicPlan;
--DELETE StudentProfile;

--clear program record
DELETE ProgramApplicationAttachment;
DELETE ProgramAttachment;
DELETE ProgramOptionValue;
DELETE Program;
DELETE ProgramType;

END TRY
BEGIN CATCH
    SELECT 
        ERROR_NUMBER() AS ErrorNumber
        ,ERROR_SEVERITY() AS ErrorSeverity
        ,ERROR_STATE() AS ErrorState
        ,ERROR_PROCEDURE() AS ErrorProcedure
        ,ERROR_LINE() AS ErrorLine
        ,ERROR_MESSAGE() AS ErrorMessage;

    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;
END CATCH;

IF @@TRANCOUNT > 0
    COMMIT TRANSACTION;
GO

--import StudentProfile (pe_demo.csv)
--import StudentPublicExam (ex_demo.csv)
--import StudentTerm (sp_demo.csv)
--import StudentCourseHistory (co_demo.csv)
--import StudentAcademicPlan (sm_demo.csv)
--import StudentAdvisor (fs_demo.csv)
--import StudentAdmission (sm_demo.csv)
--import ExchangeOption (exchange option excel)