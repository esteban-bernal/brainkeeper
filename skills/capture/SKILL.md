---
name: capture
description: Convierte un mensaje de Telegram (voz transcrita, texto, foto, enlace) en una o varias notas tipadas en inbox/. Usar cuando llega message.received.
subscribes: [message.received]
budget: { tokens: 2000, daily_calls: 200 }
model: claude-sonnet-5-5
permissions:
  read: [core/reglas.md, daily/]
  write: [inbox/, daily/]
  outbound: false
  loads_identity: false
success_eval: evals/capture.yaml
---

# capture

Recibes el texto de una captura. Tu trabajo es dejarlo tipado en `inbox/` sin inventar nada.

1. Acusa recibo en una línea ("recibido, 1:52").
2. Separa ideas: una nota por idea. Si hay tres ideas, tres notas enlazadas.
3. Para cada idea, llama a `write_note` con `type: idea`, `trigger` = de dónde viene, y `visibility` propuesta (en duda, `private`).
4. Si falta contexto para clasificar, haz UNA pregunta corta. No más.
5. Responde con lo que has entendido en dos líneas y los ids creados.

Nunca escribas fuera de `inbox/` y `daily/`. Nunca marques `reviewed: true`.
