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

test.describe("room browser", () => {
  test("shows main actions after login", async ({ page }) => {
    await registerAndLogin(page);
    await expect(page.getByRole("button", { name: "quick play" })).toBeVisible();
    await expect(page.getByRole("button", { name: "create room" })).toBeVisible();
    await expect(page.getByRole("button", { name: "logout" })).toBeVisible();
    await expect(page.getByRole("button", { name: "my profile" })).toBeVisible();
  });

  test("create room modal opens and closes", async ({ page }) => {
    await registerAndLogin(page);
    await page.getByRole("button", { name: "create room" }).click();
    await expect(page.getByRole("heading", { name: "create room" })).toBeVisible();
    await page.getByRole("button", { name: "✕" }).click();
    await expect(page.getByRole("heading", { name: "create room" })).not.toBeVisible();
  });

  test("private room requires join code", async ({ page }) => {
    await registerAndLogin(page);
    await page.getByRole("button", { name: "create room" }).click();
    await page.getByRole("button", { name: "private" }).click();
    // submit without join code
    await page.getByRole("button", { name: "create room" }).last().click();
    await expect(page.getByText("join code required for private rooms")).toBeVisible();
  });

  test("logout returns to login screen", async ({ page }) => {
    await registerAndLogin(page);
    await page.getByRole("button", { name: "logout" }).click();
    await expect(page.getByPlaceholder("username")).toBeVisible();
    await expect(page.getByRole("button", { name: "play" })).toBeVisible();
  });

  test("my profile navigates to profile page", async ({ page }) => {
    await registerAndLogin(page);
    await page.getByRole("button", { name: "my profile" }).click();
    await expect(page.getByRole("heading", { name: "stats" })).toBeVisible({ timeout: 10_000 });
  });

  test("settings menu opens and closes", async ({ page }) => {
    await registerAndLogin(page);
    await page.getByRole("button", { name: "settings" }).click();
    await expect(page.getByRole("heading", { name: "settings" })).toBeVisible();
    await page.getByRole("button", { name: "back" }).click();
    await expect(page.getByRole("heading", { name: "settings" })).not.toBeVisible();
  });
});
