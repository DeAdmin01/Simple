1. Полная стоимость всех заказов для каждого покупателя
Этот запрос проверяет навык построения длинных цепочек связей (JOIN) и базовую группировку (GROUP BY).
---
SELECT 
    sb.SalesmenBuyerName, 
    SUM(op.Quantity * s.Quantity * pm.Price) AS [Полная стоимость всех заказов]
FROM Orders o
JOIN SalesmenBuyers sb ON sb.SalesmenBuyerID = o.BuyerID
JOIN OrderProducts op ON o.OrderID = op.OrderID
JOIN Specifications s ON op.ProductID = s.ProductID
JOIN ProductMaterials pm ON pm.ProductMaterialID = s.MaterialID
GROUP BY 
    sb.SalesmenBuyerName;
---


2. Процент загрузки номерного фонда за май 2026 года
Этот запрос проверяет умение работать с функциями дат (DATEDIFF) и использовать независимые подзапросы для вычисления общих показателей.
---
SELECT 
    SUM(DATEDIFF(day, r.EntryDate, r.DepartureDate)) / ((SELECT COUNT(*) FROM Rooms) * 31.0) * 100.0 AS [Процент загрузки %]
FROM Reservations r
WHERE r.EntryDate >= '2026-05-01' AND r.DepartureDate <= '2026-05-31';
---


3.Условие (Схема Производства): Выведите имена покупателей, общая стоимость заказов которых превышает 500,000.
Почему это могут спросить: Преподаватели всегда проверяют, знаешь ли ты разницу между WHERE (фильтрует строки до группировки) и HAVING (фильтрует результаты после агрегации SUM, COUNT и т.д.).
💡 Решение: Мы берем наш первый запрос и просто добавляем условие HAVING в самый конец.
---
SELECT 
    sb.SalesmenBuyerName, 
    SUM(op.Quantity * s.Quantity * pm.Price) AS [Полная стоимость всех заказов]
FROM Orders o
JOIN SalesmenBuyers sb ON sb.SalesmenBuyerID = o.BuyerID
JOIN OrderProducts op ON o.OrderID = op.OrderID
JOIN Specifications s ON op.ProductID = s.ProductID
JOIN ProductMaterials pm ON pm.ProductMaterialID = s.MaterialID
GROUP BY 
    sb.SalesmenBuyerName
HAVING 
    SUM(op.Quantity * s.Quantity * pm.Price) > 500000;
---


4.Условие (Схема Отеля): Выведите список номеров (RoomID и название категории), которые ни разу не бронировались за всю историю отеля.
Почему это могут спросить: Это классическая задача на "поиск сирот" (orphan records). Она проверяет умение использовать внешнее соединение LEFT JOIN в связке с проверкой на IS NULL.
💡 Решение: Мы берем все номера, присоединяем к ним бронирования через LEFT JOIN. Если номер ни разу не бронировали, на месте данных о бронировании будет пустота (NULL). 
Мы фильтруем такие строки через WHERE.

SELECT 
    rm.RoomID, 
    c.CategoryName
FROM Rooms rm
JOIN Categories c ON rm.RoomCategory = c.CategoryID
LEFT JOIN Reservations r ON rm.RoomID = r.RoomID
WHERE 
    r.ReservationID IS NULL;