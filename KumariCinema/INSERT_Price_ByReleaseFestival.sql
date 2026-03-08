-- Seed Price table for booking: one row per (IsReleasePeriod, IsFestiveSeason).
-- Normal (0,0) | Release week (1,0) | Festival (0,1) | Release+Festival (1,1)
-- Run once. If Price already has rows, skip or adjust IDs.
BEGIN
  INSERT INTO Price (PriceID, PriceAmount, IsReleasePeriod, IsFestiveSeason) VALUES (1, 250, 0, 0);
EXCEPTION WHEN DUP_VAL_ON_INDEX THEN NULL;
END;
/
BEGIN
  INSERT INTO Price (PriceID, PriceAmount, IsReleasePeriod, IsFestiveSeason) VALUES (2, 300, 1, 0);
EXCEPTION WHEN DUP_VAL_ON_INDEX THEN NULL;
END;
/
BEGIN
  INSERT INTO Price (PriceID, PriceAmount, IsReleasePeriod, IsFestiveSeason) VALUES (3, 320, 0, 1);
EXCEPTION WHEN DUP_VAL_ON_INDEX THEN NULL;
END;
/
BEGIN
  INSERT INTO Price (PriceID, PriceAmount, IsReleasePeriod, IsFestiveSeason) VALUES (4, 380, 1, 1);
EXCEPTION WHEN DUP_VAL_ON_INDEX THEN NULL;
END;
/
COMMIT;
