-- Fix image paths to use frontend public folder
UPDATE Games SET ImageUrl = REPLACE(ImageUrl, '/images/games/', '/game_images/');
