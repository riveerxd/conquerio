import { test, expect, type Page } from "@playwright/test";

const uid = () => `t${Date.now()}${Math.floor(Math.random() * 1000)}`;

async function registerAndLogin(page: Page) {
  const user = uid();
  await page.goto("/");
  await page.getByText("no account? register").click();
  await page.getByPlaceholder("username").fill(user);
  await page.getByPlaceholder("email").fill(`${user}@test.com`);
  await page.getByPlaceholder("password").fill("testpass1");
  await page.getByRole("button", { name: "register" }).click();
  await expect(page.getByRole("button", { name: "quick play" })).toBeVisible({ timeout: 10_000 });
}

test.describe("game connection", () => {
  test("quick play loads game canvas", async ({ page }) => {
    await registerAndLogin(page);
    await page.getByRole("button", { name: "quick play" }).click();
    await expect(page.locator("canvas")).toBeVisible({ timeout: 15_000 });
  });

  test("disconnect returns to room browser", async ({ page }) => {
    await registerAndLogin(page);
    await page.getByRole("button", { name: "quick play" }).click();
    await expect(page.locator("canvas")).toBeVisible({ timeout: 15_000 });

    // open pause menu and disconnect
    await page.keyboard.press("Escape");
    await expect(page.getByRole("heading", { name: "menu" })).toBeVisible({ timeout: 5_000 });
    await page.getByRole("button", { name: "disconnect" }).click();
    await expect(page.getByRole("button", { name: "quick play" })).toBeVisible({ timeout: 10_000 });
  });
});
