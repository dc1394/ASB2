namespace MyLogic

module Pow =
    /// <summary>
    /// xのn乗を計算する
    /// </summary>
    /// <param name="x">xの値</param>
    /// <param name="x">nの値</param>
    /// <returns>xのn乗の値</returns>
    let pow x n = let mutable loop, m, p, y = true, n, x, 1L
                        
                  while loop do
                    if m &&& 1u <> 0u then
                        y <- y * p
            
                    m <- (m >>> 1)

                    if m = 0u then
                        loop <- false

                    if loop then
                        p <- p * p
                  y