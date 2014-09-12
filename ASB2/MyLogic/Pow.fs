namespace MyLogic

module Pow =
    let pow x n = let mutable m, p, y = n, x, 1L
                  let mutable loop = true
                        
                  while loop do
                    if m &&& 1u <> 0u then
                        y <- y * p
            
                    m <- (m >>> 1)

                    if m = 0u then
                        loop <- false

                    if loop then
                        p <- p * p
                  y