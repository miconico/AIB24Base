-- ================================================
-- Template generated from Template Explorer using:
-- Create Procedure (New Menu).SQL
--
-- Use the Specify Values for Template Parameters 
-- command (Ctrl-Shift-M) to fill in the parameter 
-- values below.
--
-- This block of comments will not be included in
-- the definition of the procedure.
-- ================================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,m2dcno>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE spiGetBalance 
	-- Add the parameters for the stored procedure here
	@Current188 as money,
    @Savings261 as money,
    @Savings345 as money,
    @OnlineSavings501 as money,
    @Current076 as money,
    @OnlineSavings159 as money,
    @AccountsBalance as money,
    @MoneyIn as money,
    @Spend as money
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
    -- Insert statements for procedure here
	INSERT INTO BudgetSpend VALUES (@Current188, @Savings261, @Savings345, @OnlineSavings501, @Current076, @OnlineSavings159, @AccountsBalance, GETDATE(),1,@MoneyIn, @Spend)
	
END
GO
