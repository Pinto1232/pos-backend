-- Fix the Type column in Scope table using USING clause
ALTER TABLE "Scope" 
ALTER COLUMN "Type" TYPE integer 
USING CASE 
    WHEN "Type" = 'Global' THEN 0
    WHEN "Type" = 'Store' THEN 1
    WHEN "Type" = 'Terminal' THEN 2
    ELSE 0 -- Default to Global if unknown
END;
