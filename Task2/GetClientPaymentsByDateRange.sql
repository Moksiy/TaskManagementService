CREATE OR ALTER FUNCTION dbo.GetClientPaymentsByDateRange (
    @ClientId BIGINT,
    @Sd DATE,
    @Ed DATE
)
RETURNS TABLE
AS
RETURN
    WITH DateRange AS (
        SELECT CAST(@Sd AS DATE) AS [Date]
        UNION ALL
        SELECT DATEADD(DAY, 1, [Date])
        FROM DateRange
        WHERE DATEADD(DAY, 1, [Date]) <= @Ed
    )

    SELECT 
        CAST(dr.[Date] AS DATE) AS Dt,
        COALESCE(SUM(cp.Amount), 0) AS [Сумма]
    FROM DateRange dr
    LEFT JOIN ClientPayments cp 
		ON CAST(cp.Dt AS DATE) = dr.[Date] 
        AND cp.ClientId = @ClientId
    GROUP BY dr.[Date];